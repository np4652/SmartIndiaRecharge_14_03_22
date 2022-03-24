using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Report
{
    public class PrepairedTransactionReq
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public int WID { get; set; }
        public string TransactionID { get; set; }
        public string CustomerNumber { get; set; }
        public string Operator { get; set; }
        public string O15 { get; set; }
        public bool IsBBPS { get; set; }
        public int UserID { get; set; }
        public string AccountNo { get; set; }
        public decimal RequestedAmount { get; set; }
        public int OID { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string ExtraParam { get; set; }
        public APIDetail aPIDetail { get; set; }

        //for DMR
        public string SenderNo { get; set; }
        public string BeneID { get; set; }
        public string TransactionMode { get; set; }
        public string OutletPincode { get; set; }
        public string OutletLatLong { get; set; }
        public string APIOutletID { get; set; }
        public int APIID { get; set; }
        public string API { get; set; }
        public string APICode { get; set; }
        public string PAN { get; set; }
        public string Aadhar { get; set; }
    }
}
