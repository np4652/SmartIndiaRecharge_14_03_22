namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class GETStatusCheckURL
    {
        public int OID { get; set; }
        public string GroupIID { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string RequestURL { get; set; }
        public string TransactionID { get; set; }
        public string Operator { get; set; }
        public string CustomerNumber { get; set; }
        public string VendorID { get; set; }
        public int _Type { get; set; }
        public bool IsBBPS { get; set; }
        public int TID { get; set; }
        public RechargeAPIHit RechargeAPI { get; set; }
        public int UserID { get; set; }
        public int WID { get; set; }
        public int RequestMode { get; set; }
        public decimal AmountR { get; set; }
        public string Optional2 { get; set; }
        public string Account { get; set; }
        public string SenderName { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string APIContext { get; set; }
        public string TDate { get; set; }
        public string SCode { get; set; }
        public int OutletID { get; set; }
        public string Response { get; set; }
        public string BrandName { get; set; }
        public string APIOutletID { get; set; }
        public string VendorID2 { get; set; }
        public string TransactionReqID { get; set; }
        public string SPKey { get; set; }
        public string APIOPCode { get; set; }
    }
}
