using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class ReferralCommission : Controller
    {
        public int ID { get; set; }
        public string TopUpName{ get; set; }
        public int IsGreaterThan { get; set; }
        public int Comm { get; set; }
        public bool IsActive{ get; set; }
        public int CommOnReg{ get; set; }
    }
}
