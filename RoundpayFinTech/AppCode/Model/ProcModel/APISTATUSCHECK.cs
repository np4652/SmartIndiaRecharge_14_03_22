using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class APISTATUSCHECK
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ID { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Checks { get; set; }
        public int Status { get; set; }
        public int? VendorIDIndex { get; set; }
        public string VendorID { get; set; }
        public string VendorIDReplace { get; set; }
        public int? OperatorIDIndex { get; set; }
        public string OperatorID { get; set; }
        public string OperatorIDReplace { get; set; }
        public int? TransactionIDIndex { get; set; }
        public string TransactionID { get; set; }
        public string TransactionIDReplace { get; set; }
        public int? BalanceIndex { get; set; }
        public string Balance { get; set; }
        public string BalanceReplace { get; set; }
        public int IndLength { get; set; }
        public List<string> SplitMsg { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
    }
}
