using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class TransactionMode
    {
        public string TransMode { get; set; }
        public string Code { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public decimal Charge { get; set; }
    }

    public class WalletRequest: CommonReq
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public string UserRoleId { get; set; }
        public int ShowGroupID { get; set; }
        public decimal Amount { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
        public string EntryDate { get; set; }
        public string ApprovalDate { get; set; }
        public string TransMode { get; set; }
        public decimal Charge { get; set; }
        public string UserName { get; set; }
        public int Status { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string AccountHolder { get; set; }
        public string AccountNumber { get; set; }
        public string FDate { get; set; }
        public string TDate { get; set; }
        public int GroupID { get; set; }
        public string TransactionId { get; set; }
        public string TID { get; set; }
        public List<WalletRequest> PayIds { get; set; }
        public DataTable dt { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Email { get; set; }
        public string BankRRN { get; set; }
        public decimal MiniBankBalance { get; set; }
        public decimal MiniBankCapping { get; set; }
        public bool InRealTime { get; set; }
    }
}
