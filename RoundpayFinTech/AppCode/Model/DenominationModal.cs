using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class DenominationModal
    {
        public int ID { get; set; }
        public int Amount { get; set; }
        public int EntryBy { get; set; }
        public string EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public string Remark { get; set; }
        public int IsDel { get; set; }
    }
    public class DenominationModalReq: CommonReq
    {
        public DenominationModal Detail { get; set; }
    }
}
