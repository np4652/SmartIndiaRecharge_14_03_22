using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class EmpDownlineUser
    {
        public string Prefix { get; set; }
        public int UserID { get; set; }
        public string UserMobile { get; set; }
        public string UserName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Role { get; set; }
        public bool Attandance{ get; set; }
    }
}
