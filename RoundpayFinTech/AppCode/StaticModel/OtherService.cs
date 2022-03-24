
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.StaticModel
{
    public class ServiceTransactionRequest
    {
        public int UserID { get; set; }
        public string OutletID { get; set; }
        public string Token { get; set; }
        public string AccountNo { get; set; }
        public decimal AmountR { get; set; }
        public string APIRequestID { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
        public int APIID { get; set; }
        public int OPType { get; set; }
        public string IMEI { get; set; }
        public string RequestRespones { get; set; }
        public string VenderID { get; set; }
        public string OTP { get; set; }
        public string RequestSession { get; set; }
        public int RefferenceID { get; set; }
        public bool IsBalance { get; set; }
    }
    public class RailwayRespones
    {
        public string UserID { get; set; }
        public string TranID { get; set; }
        public DateTime TranDate { get; set; }
        public decimal TranAmount { get; set; }
        public string PGCHARGE { get; set; }
        public string BalanceRequestResponse { get; set; }
        public string CoRelationID { get; set; }
    }
}
