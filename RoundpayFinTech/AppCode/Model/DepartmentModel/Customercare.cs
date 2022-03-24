using Microsoft.AspNetCore.Mvc.Rendering;

namespace RoundpayFinTech.AppCode.Model.DepartmentModel
{
    public class Customercare
    {
        public int ID { get; set; }
        public int DeptID { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public int CityID { get; set; }
        public int StateID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public bool IsActive { get; set; }
        public bool IsOTP { get; set; }
        public string Prefix { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public string Password { get; set; }
    }
    public class CustomerCareEdit
    {
        public SelectList DDLDepartment { get; set; }
        public SelectList DDLDeptRole { get; set; }
        public Customercare CustomercareInfo { get; set; }
    }
}
