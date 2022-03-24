using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class UserReport : UserBalnace
    {
        public bool IsPaymentGateway { get; set; }
        public bool IsDownLinePG { get; set; }
        public bool IsECollection { get; set; }
        public bool IsCalculateCommissionFromCircle { get; set; }
        public int ID { get; set; }
        public string Role { get; set; }
        public string KYCStatus { get; set; }
        public int KYCStatus_ { get; set; }
        public string RentalStatus { get; set; }
        public string PackageName { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string WhatsAppNumber { get; set; }
        public string EMail { get; set; }
        public bool IsEmailVerified { get; set; }
        public string EmailVerifiedStatus { get; set; }
        public bool Status { get; set; }
        public bool IsOTP { get; set; }
        public string JoinDate { get; set; }
        public string JoinBy { get; set; }
        public string Slab { get; set; }
        public string WebsiteName { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int RoleID { get; set; }
        public int IntroID { get; set; }
        public int FOSId { get; set; }
        public string FOSName { get; set; }
        public string FOSMobile { get; set; }
        public string JoinByMobile { get; set; }
        public string UserArea { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsFlatCommission { get; set; }
        public int EmpID { get; set; }
        public bool IsAutoBilling { get; set; }
        public int MaxBillingCountAB { get; set; }
        public int BalanceForAB { get; set; }
        public int AlertBalance { get; set; }
        public bool FromFOSAB { get; set; }
        public int MaxCreditLimitAB { get; set; }
        public int MaxTransferLimitAB { get; set; }
        public bool Candebit { get; set; }
        public bool CandebitDownline { get; set; }
        public bool IsGoogle2FAEnable { get; set; }
        public string AccountSecretKey { get; set; }
        public string introducerName { get; set; }
        public string introducerMobile { get; set; }
    }
    public class AutoBillingModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public int LoginID { get; set; }
        public int LT { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsAutoBilling { get; set; }
        public int MaxBillingCountAB { get; set; }
        public int BalanceForAB { get; set; }
        public int AlertBalance { get; set; }
        public bool FromFOSAB { get; set; }
        public int MaxCreditLimitAB { get; set; }
        public int MaxTransferLimitAB { get; set; }
        public string UserIdBulk { get; set; }
      
    }
    public class UserList
    {
        public bool CanChangeUserStatus { get; set; }
        public bool CanCalculateCommissionFromCircle { get; set; }
        public List<UserReport> userReports { get; set; }
        public bool CanEdit { get; set; }
        public bool CanAssignPackage { get; set; }
        public bool CanVerifyDocs { get; set; }
        public bool CanFundTransfer { get; set; }
        public bool CanChangeOTPStatus { get; set; }
        public bool CanChangeSlab { get; set; }
        public bool CanChangeRole { get; set; }
        public bool CanAssignAvailablePackage { get; set; }
        public int LoginID { get; set; }
        public int? RowCount { get; set; }
        public PegeSetting PegeSetting { get; set; }
        public bool CanRegeneratePassword { get; set; }

        public bool Candebit { get; set; }
        public bool CandebitDownline { get; set; }
    }
    public class UserRequest : CommonFilter
    {
        public string Name { get; set; }
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public int Status { get; set; }
        public int FOSId { get; set; }
        public List<int> UserIds { get; set; }
        public DataTable dt { get; set; }
        public string SlabName { get; set; }
    }
    public class StateMaster
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
    }
    public class GetEditUser : UserInfo
    {
        public int CompanyTypeID { get; set; }
        public SelectList CompanyTypeSelect { get; set; }
        public decimal CommRate { get; set; }
        public int DistrictID { get; set; }
        public string ProfilePic { get; set; }
        public int KYCStatus { get; set; }
        public string AADHAR { get; set; }
        public string PAN { get; set; }
        public string GSTIN { get; set; }
        public string PartnerName { get; set; }
        public string Address { get; set; }
        public int LoginID { get; set; }
        public int LT { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<SlabMaster> Slabs { get; set; }
        public List<StateMaster> States { get; set; }
        public SelectList Bankselect { get; set; }
        public SelectList DMRModelSelect { get; set; }
        //public string Emailid { get; set; }
        public string DOB { get; set; }
        public string ShopType { get; set; }
        public string Qualification { get; set; }
        public string Poupulation { get; set; }
        public string LocationType { get; set; }
        public string Landmark { get; set; }
        public string AlternateMobile { get; set; }
        public string Latlong { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string IFSC { get; set; }
        public string AccountNumber { get; set; }
        public string WhatsAppNumber { get; set; }
        public string AccountName { get; set; }
        public string Requestdate { get; set; }
        public bool IsOutsider { get; set; }
        public string WebsiteName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBankUpdateAvailable { get; set; }
        public bool IsRegisteredWithGST { get; set; }
        public bool IsLoginWebsite { get; set; }
        public string CustomLoginID { get; set; }
        public List<Cards> Cards { get; set; }
        public string DisplayName { get; set; }
        public string DisplayID { get; set; }

    }
    public class UserReportBulk : UserReport
    {
        public string Rental { get; set; }
        public decimal RentalAmt { get; set; }
        public decimal Capping { get; set; }
        public int ReferalID { get; set; }
    }
    public class FOSUserExcelModel
    {
        public string OutletName { get; set; }
        public string OutletMobile { get; set; }
        public decimal PrepaidBalance { get; set; }
        public decimal UtilityBalance { get; set; }
        public string Slab { get; set; }
        public string JoinDate { get; set; }
        public string JoinBy { get; set; }
        public string KYCStatus { get; set; }
        public string FOSName { get; set; }
        public string FOSMobile { get; set; }
    }
    public class Cards
    {
        public int ID { get; set; }
        public string CardNumber { get; set; }
        public string ValidFrom { get; set; }
        public string ValidThru { get; set; }
    }
}
