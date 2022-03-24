using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.DepartmentModel
{
    public class Department
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
    }
    public class DepartmentRole
    {
        public int ID { get; set; }
        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public string Prefix { get; set; }
        public string DepartmentName { get; set; }
        public string ModifyDate { get; set; }
    }
    public class DepartmentEntity
    {
        public DepartmentRole departmentRole { get; set; }
        public SelectList selectDepartment { get; set; }
    }
}
