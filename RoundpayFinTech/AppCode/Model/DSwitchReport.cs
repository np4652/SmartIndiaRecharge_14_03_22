using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DSwitchReport
    {
        public int OID { get; set; }
        public int APIID { get; set; }
        public int RoleID { get; set; }
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Prefix { get; set; }
        public string Role { get; set; }
        public string Operator { get; set; }
        public string API { get; set; }
        
        public string Denoms { get; set; }
        public string DenomR { get; set; }

        public string CircleName { get; set; }
    }

    public class DSRDesign
    {
        public int OID { get; set; }
        public string Operator { get; set; }
        public int RoleID { get; set; }
        public int UserID { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string Prefix { get; set; }
        public string Role { get; set; }
        public List<DSwitchReport> DSList { get; set; }

        public string CircleName { get; set; }
    }
}
