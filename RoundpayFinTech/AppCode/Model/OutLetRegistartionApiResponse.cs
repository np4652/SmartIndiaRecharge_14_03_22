namespace RoundpayFinTech.AppCode.Model
{
    public class OutletAPIStatusUpdate
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsEKYCAlreadyVerified { get; set; }
        public bool IsOnboardedOnAPI { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int OTPRefID { get; set; }
        public string APIOutletID { get; set; }
        public int APIOutletStatus { get; set; }
        public int KYCStatus { get; set; }
        public string BBPSID { get; set; }
        public int BBPSStatus { get; set; }
        public string AEPSID { get; set; }
        public int AEPSStatus { get; set; }
        public string DMTID { get; set; }        
        public int DMTStatus { get; set; }
        public string RailID { get; set; }
        public int RailStatus { get; set; }
        public int BioAuthType { get; set; }
        public int PSARequestID { get; set; }
        public string PSAID { get; set; }
        public int PSAStatus { get; set; }
        public string AEPSURL { get; set; }
        public string APIReferenceID { get; set; }
        public string APIHash { get; set; }
        public string MyPartnerIDInAPI { get; set; }
    }

    public class OutletAPIStatusUpdateReq : OutletAPIStatusUpdate
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int APIID { get; set; }
        public string _APICode { get; set; }
        public bool IsVerifyStatusUpdate { get; set; }
        public bool IsDocVerifyStatusUpdate { get; set; }
        public bool IsBBPSUpdate { get; set; }
        public bool IsBBPSUpdateStatus { get; set; }
        public bool IsAEPSUpdate { get; set; }
        public bool IsAEPSUpdateStatus { get; set; }
        public bool IsPANRequestIDUpdate { get; set; }
        public bool IsPANUpdate { get; set; }
        public bool IsPANUpdateStatus { get; set; }
        public bool IsDMTUpdate { get; set; }
        public bool IsDMTUpdateStatus { get; set; }
        public bool IsRailIDUpdate { get; set; }
        public bool IsRailUpdateStatus { get; set; }
    }    
}
