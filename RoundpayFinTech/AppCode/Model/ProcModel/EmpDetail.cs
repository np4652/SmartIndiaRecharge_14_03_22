using RoundpayFinTech.AppCode.Model.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    
    public class EmpRequest
    {
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        
    }
}
