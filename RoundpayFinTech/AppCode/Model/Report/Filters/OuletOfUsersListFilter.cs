
namespace RoundpayFinTech.AppCode.Model.Reports.Filter
{
    public class OuletOfUsersListFilter /*: RechargeReportFilter*/
    {
        public int TopRows { get; set; }
        public string MobileOrUserID { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public string Name { get; set; }
        public string PAN { get; set; }
        public string Adhar { get; set; }
        
        public int KycStatus { get; set; }
        public int VerifyStatus { get; set; }
        public bool IsExport { get; set; }
        public int ApiId { get; set; }
        public int ApiStatus { get; set; }
        public int ServiceId { get; set; }
        public int ServiceStatusId { get; set; }
    }

    public class _OuletOfUsersListFilter : OuletOfUsersListFilter
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int OutletID { get; set; }
        public string OutletMobile { get; set; }
        public string UserOutletMobile { get; set; }
        public int UserID { get; set; }
        public string Mobile_F { get; set; }
        public string DeviceId { get; set; }
    }
}
