using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class EmployeeList
    {
        public List<EList> Employees { get; set; }
        public PegeSetting PegeSetting { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public bool IsAdmin { get; set; }
        public int LoginID { get; set; }
    }

    public class EmpReg : EList
    {
        public List<RoleMaster> Roles { get; set; }
    }

    public class EList
    {
        public int EmpID { get; set; }
        public string EmpCode { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingToName { get; set; }
        public string Password { get; set; }

        public int EmpRoleID { get; set; }
        public string EmpRole { get; set; }
        public string Prefix { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public int PinCode { get; set; }
        public string PAN { get; set; }
        public string AADHAR { get; set; }
        public bool IsActive { get; set; }
        public bool IsOtp { get; set; }
        public int EntryBy { get; set; }
        public int ReferralID { get; set; }
        public string ReferralBy { get; set; }
    }
}
