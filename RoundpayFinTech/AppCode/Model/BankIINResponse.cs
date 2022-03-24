using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    
    public class BankIINDetail
    {
        public int id { get; set; }
        public int activeFlag { get; set; }
        public string bankName { get; set; }
        public string details { get; set; }
        public object remarks { get; set; }
        public string timestamp { get; set; }
        public string iinno { get; set; }
    }

    public class BankIINResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public List<BankIINDetail> data { get; set; }
        public int statusCode { get; set; }
    }

}
