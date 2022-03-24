using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class CommonRequest
    {
        public int LoginId { get; set; }
        public int LT { get; set; }
        public int CommonInt { get; set; }
        public int CommonInt2 { get; set; }
        public int CommonInt3 { get; set; }
        public string CommonStr { get; set; }
        public string CommonStr2 { get; set; }
        public string CommonStr3 { get; set; }
        public string CommonStr4 { get; set; }
        public bool CommonBool { get; set; }
        public char Flag { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public int Status { get; set; }
    }
}
