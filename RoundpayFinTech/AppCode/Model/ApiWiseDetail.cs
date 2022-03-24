namespace RoundpayFinTech.AppCode.Model
{
    public class ApiWiseDetail
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public int _ServiceID { get; set; }
        public int ID { get; set; }
        public int OutletID { get; set; }
        public int APIID { get; set; }
        public string APIName { get; set; }
        public string APIOutletId { get; set; }
        public int VerifyStatus { get; set; }
        public string _VerifyStatus { get; set; }
        public string BBPSID { get; set; }
        public int BBPSStatus{get;set;}
        public string _BBPSStatus { get;set;}
        public string AEPSID { get; set; }
        public int AEPSStatus{get;set;}
        public string _AEPSStatus { get;set;}
        public string DMTID { get; set; }
        public int DMTStatus{get;set;}
        public string _DMTStatus { get;set;}
        public string PSAID { get; set; }
        public int PSAStatus { get; set; }
        public string _PSAStatus { get; set; }
        public string LastUpdatedOn { get; set; }
    }
}
