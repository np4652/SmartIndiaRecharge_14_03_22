using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Lookup
{
    public class LookUpRes
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string Mobile { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public string CircleCode { get; set; }
        public string Message { get; set; }
    }

    public class LookUpDBLogReq
    {
        public int LoginID { get; set; }
        public string Mobile { get; set; }
        public string CurrentOperator { get; set; }
        public string CurrentCircle { get; set; }
        public string Response { get; set; }
        public string Request { get; set; }
        public int APIID { get; set; }
        public string APIType { get; set; }
        public bool IsCircleOnly { get; set; }
    }
}
