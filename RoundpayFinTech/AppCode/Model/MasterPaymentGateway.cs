using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class MasterPaymentGateway
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string StatusCheckURL { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public string Code { get; set; }
        public bool IsUPI { get; set; }
        public bool IsLive { get; set; }
    }
}
