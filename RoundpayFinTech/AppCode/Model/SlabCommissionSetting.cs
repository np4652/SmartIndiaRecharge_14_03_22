using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SlabCommissionSetting
    {
        public IEnumerable<OpTypeMaster> OpTypes { get; set; }
        public IEnumerable<OperatorDetail> Operators { get; set; }
    }

    public class SlabCommissionSettingRes
    {
        public int ID { get; set; }
        public int SlabID { get; set; }
        public int OID { get; set; }
        public string SlabName { get; set; }
        public decimal Commission { get; set; }
        public int CommissionType { get; set; }
        public int AmountType { get; set; }
        public decimal RComm { get; set; }
        public int RCommType { get; set; }
        public int RAmtType { get; set; }
    }
}
