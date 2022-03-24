using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.Model
{
    public class SlabCommissionReq:CommonReq
    {
        public int SlabID { get; set; }
        public int OID { get; set; }
        public int OPID { get; set; }
        public int RoleID { get; set; }
        public string Action { get; set; }
        public string Mode { get; set; }
        public string IsGenralOrReal { get; set; }
        public decimal Amount { get; set; }
        public bool CommissionType { get; set; }
        public bool AmountType { get; set; }
    }
}
