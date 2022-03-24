using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ICollect
{
    public class ICollectStatusResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string Amount { get; set; }
        public string UTR { get; set; }
    }
}
