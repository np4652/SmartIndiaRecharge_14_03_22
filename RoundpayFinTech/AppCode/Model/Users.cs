
namespace RoundpayFinTech.AppCode.Model
{
    public class MasterCompanyType
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
    }
    public class Users
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public int PinCode { get; set; }
        public int ReferalID { get; set; }
        public decimal Balance { get; set; }
        public string IntroducerName { get; set; }
        public string IntroducerMobile { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public bool IsViewed { get; set; }
    }
}
