using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class BlockSameRequest
    {
        public string Method { get; set; }
        public string Request { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
}
