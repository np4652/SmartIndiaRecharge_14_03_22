namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class RechargeRequest
    {
        public int OID { get; set; }
        public string Mob { get; set; }
        public decimal Amt { get; set; }
        public string O1 { get; set; }
        public string O2 { get; set; }
        public string O3 { get; set; }
        public string O4 { get; set; }
        public string CustNo { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string ReferenceID { get; set; }
        public string SecKey { get; set; }
        public int OutletID { get; set; }
        public int FetchBillID { get; set; }
        public int CCFAmount { get; set; }
        public string PaymentMode { get; set; }
    }
    public class AppRechargeRequest : RechargeRequest
    {
        public int UserID { get; set; }
        public string IMEI { get; set; }
        public string GEOCode { get; set; }
        public string Pincode { get; set; }
        public string RefID { get; set; }
        public int RequestMode { get; set; }
        public bool IsReal { get; set; }
        public int PromoCodeID { get; set; }

    }
    public class DthConnect
    {
        public int OID { get; set; }
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
    }
}
