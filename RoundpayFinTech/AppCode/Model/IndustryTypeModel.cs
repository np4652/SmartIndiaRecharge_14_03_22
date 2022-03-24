using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class IndustryTypeModel
    {
        public int ID { get; set; }
        public string IndustryType { get; set; }
        public string Remark { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
    }

    public class IndustryTypeModelProc
    {
        public int ID { get; set; }
        public string IndustryType { get; set; }
        public int Ind { get; set; }
        public string Remark { get; set; }
        public int OpTypeID{ get; set; }
        public string OpType{ get; set; }
    }
}
