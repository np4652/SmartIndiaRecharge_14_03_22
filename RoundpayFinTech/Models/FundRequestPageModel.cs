using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class FundRequestPageModel
    {
        public List<Bank> bankList { get; set; }
        public List<UserSmartDetail> userSmartDetail { get; set; }
    }
}
