using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class CoinQRViewModel
    {
        public IEnumerable<OperatorDetail> opList{ get; set; }
        public string Host{ get; set; }
        public string QRAddress { get; set; }
    }
}
