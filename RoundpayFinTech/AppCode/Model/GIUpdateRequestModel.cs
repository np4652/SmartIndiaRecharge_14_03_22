namespace RoundpayFinTech.AppCode.Model
{
    public class GIUpdateRequestModel
    {
        public int TID { get; set; }
        public int Status { get; set; }
        public string APICode { get; set; }
        public string APIOutletID { get; set; }
        public string APIOpCode { get; set; }
        public string TransactionID { get; set; }
        public int OutletID { get; set; }   
        public string RechType { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal ODAmount { get; set; }
        public decimal Payout { get; set; }
        public string AccountNo { get; set; }
        public string RequestIP { get; set; }

    }
    public class GIAPIWaleUpdateModel {
        public string SessionKey { get; set; }
        public string TranId { get; set; }
        public int UserCode { get; set; }
        public string ServiceCode { get; set; }
        public string TranRef { get; set; }
        public string RelatedRef { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal OD { get; set; }
        public int status { get; set; }
        public string insurancetype { get; set; }
        public string Description { get; set; }
    }
}
