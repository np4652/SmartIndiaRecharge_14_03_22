
namespace RoundpayFinTech.AppCode.Model.ProcModel
{
	public class CompanyProfileDetail
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public string EmailId { get; set; }
		public string PhoneNo { get; set; }
		public string mobileNo { get; set; }
        public string mobileNo2 { get; set; }
        public string AccountMobileNo { get; set; }
		public string AccountEmailId { get; set; }

        public string Facebook { get; set; }
		public string Instagram { get; set; }
		public string Twitter { get; set; }
		public string WhatsApp { get; set; }
		public string website { get; set; }
        public string PaymentEnquiry { get; set; }
        public string HeaderTitle { get; set; }

        public string CustomerCareMobileNos { get; set; }        public string CustomerCareEmailIds { get; set; }        public string CustomerPhoneNos { get; set; }        public string CustomerWhatsAppNos { get; set; }        public string AccountPhoneNos { get; set; }        public string AccountWhatsAppNos { get; set; }
        public string SalesPersonNo { get; set; }
        public string SalesPersonEmail { get; set; }
        public string AppName { get; set; }
        public string OwnerName { get; set; }
        public string OwnerDesignation { get; set; }
        public int Statuscode { get; set; }
        public int KYCStatus { get; set; }
        public string Msg { get; set; }
        public string Pan { get; set; }
        public string UserAddress { get; set; }
        public int SignupReferalID { get; set; }
        public bool IsOutletRequired { get; set; }
    }
    public class CompanyProfileDetailReq: CompanyProfileDetail
    {
        public string EmailIdSupport { get; set; }
        public int WID { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
    }
}
