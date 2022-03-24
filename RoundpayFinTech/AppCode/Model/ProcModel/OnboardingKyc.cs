using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class OnboardingKyc
    {
        public string Adhaar { get; set; }
        public string Pan { get; set; }
        public string ShopPhoto { get; set; }
        public string PassportPhoto { get; set; }
        public string CancelledCheque { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Doctypeid { get; set; }
    }
}
