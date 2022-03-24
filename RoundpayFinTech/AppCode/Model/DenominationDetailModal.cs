using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class DenominationDetailModal
    {
        public int OpTypeID { get; set; }
        public int SlabID { get; set; }
        public int APIID { get; set; }
        public bool IsAdminDefined { get; set; }
        public List<OperatorDetail> Operator { get; set; }
        public List<DenominationModal> DenomList { get; set; }
        public List<DenominationRange> DenomRangeList { get; set; }
        public IEnumerable<OpTypeMaster> OpTypes { get; set; }
        public IEnumerable<CirlceMaster> CirlceList { get; set; }
        public IEnumerable<CircleWithDomination> circleWithDominations { get; set; }
    }
    public class CircleWithDomination
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public int OpTypeID { get; set; }
        public int ID { get; set; }
        public int APIID { get; set; }
        public int CircleID { get; set; }
        public string CircleName { get; set; }
        public int AmtType { get; set; }
        public int CommType { get; set; }
        public int IsActive { get; set; }
        public decimal Comm { get; set; }
        public int IsDenom { get; set; }
        public int DenomID { get; set; }
        public int DenomRangeID { get; set; }
        public string DenomIDs { get; set; }
        public string DenomRangeIDs { get; set; }
        public string Amount { get; set; }
        public int OID { get; set; }
        public int SlabID { get; set; }
        public string ModificationDate { get; set; }
        public int DominationType { get; set; }
    }

}
