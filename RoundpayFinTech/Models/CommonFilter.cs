namespace RoundpayFinTech.AppCode.Model
{
    public class CommonFilter
    {
        public int UserID { get; set; }
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string MobileNo { get; set; }
        public int RoleID { get; set; }
        public int LoginRoleID { get; set; }
        public string EmailID { get; set; }
        public int EmployeeRole { get; set; }
        public string Name { get; set; }
        public bool SortByID { get; set; }
        public bool IsDesc { get; set; }
        public bool IsFOSListAdmin { get; set; }
        public int TopRows { get; set; }
        public int btnID { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }        
        public int MapStatus { get; set; }
        public int FOSID { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool IsAdminDefined { get; set; }
        public sbyte RequestMode { get; set; }
    }
}
