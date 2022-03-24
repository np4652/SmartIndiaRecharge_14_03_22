using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DMTSetting
    {
        public string partnerId { get; set; }
        public string BaseURL { get; set; }
        public string TransactionURL { get; set; }
        public string LimitURL { get; set; }
        public string appVersion { get; set; }
        public string salt { get; set; }
    }
}
