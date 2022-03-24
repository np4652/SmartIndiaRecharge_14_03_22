using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class SattlementAccountModels:CommonReq 
    {
        public int? ID { get; set; }
        public int BankID { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public string ApprovedBY { get; set; }
        public string ApprovalIp { get; set; }
        public string ApprovalDate { get; set; }
        public string Actualname { get; set; }
        public string UTR { get; set; }
        public string APIID { get; set; }
        public int? ApprovalStatus { get; set; }
        public int? VerificationStatus { get; set; }
        public string Isdeleted { get; set; }
        public bool IsDefault { get; set; }
        public string VerificationText { get; set; }
        public string ApprovalText { get; set; }


        public SelectList Bankselect { get; set; }
        public string Requestdate { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public int RequestID { get; set; }
    }

    public class SattlementAccountStatus
    {
        public IEnumerable<_Status> ApprovalStatus { get; set; }

        public IEnumerable<_Status> VerificationStatus { get; set; }



    }
    public class _Status
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
