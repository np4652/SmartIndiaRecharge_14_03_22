using System.Collections.Generic;

namespace Fintech.AppCode.StaticModel
{
    public class LoginType
    {
        
        public const int ApplicationUser = 1;
        public const int CustomerCare = 2;
        public const int Employee = 3;
        public const string _ApplicationUser = "Application User";
        public const string _CustomerCare = "Customer Care";
        public const string _Employee = "Employee";

        public static string GetLoginType(int t) {
            if (t == ApplicationUser)
                return _ApplicationUser;
            if (t == CustomerCare)
                return _CustomerCare;
            if (t == Employee)
                return _Employee;
                return "";
        }
    }
    public class Role
    {
        public const int Admin = 1;
        public const int APIUser = 2;
        public const int Retailor_Seller = 3;
        public const int FOS = 8;
        public const int MasterWL = 5;
        public const int Customer = 4;    
        public const int Distributor = 7;
        public const int Master_Distributor = 6;    
        
    }
    public class FixedRole
    {
        public const int Admin = 1;
        public const int APIUser = 2;
        public const int Retailor = 3;
        public const int SubAdmin = 5;
        public const int MasterDistributer = 6;
        public const int Distributor = 7;
        public const int FOS = 8;
    }
    public class MapStatus
    {
        public const int Unassign = 0;
        public const int All = 0;
        public const int Unassigned = 1;
        public const int Assigned = 2;
    }
    public class EmployeeRole
    {
        public const int Sales_Head = 2;
        public const int State_Head = 3;
        public const int Cluster_Head = 4;
        public const int ASM = 5;
        public const int TSM = 6;
    }

}
