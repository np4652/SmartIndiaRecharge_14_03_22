using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DenomCommissionDetail
    {
        public int DenomID { get; set; }
        public string Amount { get; set; }
        public decimal Commission { get; set; }
        public int AmtType { get; set; }
    }
}
