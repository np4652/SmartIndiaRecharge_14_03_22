
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class EmployeeListU
    {
        public List<EmpUserList> EmpReports { get; set; }
        public bool CanEdit { get; set; }
        public bool CanAssignPackage { get; set; }
        public bool CanVerifyDocs { get; set; }
        public bool CanFundTransfer { get; set; }
        public bool CanChangeUserStatus { get; set; }
        public bool CanChangeOTPStatus { get; set; }
        public bool CanChangeSlab { get; set; }
        public bool CanChangeRole { get; set; }
        public bool CanAssignAvailablePackage { get; set; }
        public int LoginID { get; set; }
        public int? RowCount { get; set; }
        public PegeSetting PegeSetting { get; set; }
        public bool CanRegeneratePassword { get; set; }
    }

    public class EmpUserBal
    {
        public decimal Balance { get; set; }
        public decimal BCapping { get; set; }
        public bool IsBalance { get; set; }
        public bool IsBalanceFund { get; set; }
        public decimal UBalance { get; set; }
        public decimal UCapping { get; set; }
        public bool IsUBalance { get; set; }
        public bool IsUBalanceFund { get; set; }
        public decimal BBalance { get; set; }
        public decimal BBCapping { get; set; }
        public bool IsBBalance { get; set; }
        public bool IsBBalanceFund { get; set; }
        public decimal CBalance { get; set; }
        public decimal CCapping { get; set; }
        public bool IsCBalance { get; set; }
        public bool IsCBalanceFund { get; set; }
        public decimal IDBalnace { get; set; }
        public decimal IDCapping { get; set; }
        public bool IsIDBalance { get; set; }
        public bool IsIDBalanceFund { get; set; }
        public decimal PacakgeBalance { get; set; }
        public decimal PackageCapping { get; set; }
        public bool IsPacakgeBalance { get; set; }
        public bool IsPacakgeBalanceFund { get; set; }
        public bool IsP { get; set; }//IsPasswordExpiredOrNot
        public bool IsPN { get; set; }//_IsPINNotSet
        public bool IsLowBalance { get; set; }
        public bool IsAdminDefined { get; set; }//Channel or Level type user
        public decimal CommRate { get; set; }
    }
    public class EmpUserList : EmpUserBal
    {
        public int ID { get; set; }
        public string Role { get; set; }
        public string KYCStatus { get; set; }
        public int KYCStatus_ { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
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
        public bool IsVirtual { get; set; }
    }
}
