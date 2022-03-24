using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{

    public class SettlementSetting
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public List<SettlementType> settlementType { get; set; }
        public UserwiseSettSetting userwiseSettSetting { get; set; }
    }

    public class SettlementType
    {
        public int ID { get; set; }
        public string SetTType { get; set; }
        public string Remark { get; set; }
    }

    public class UserwiseSettSetting
    {
        public int UserID { get; set; }
        public int MTRWSettleType { get; set; }
        public int MTRWSettleTypeMB { get; set; }
        public string MTRWSettleTypeRemark { get; set; }
        public string MTRWSettleTypeMBRemark { get; set; }
        public string AEPSType { get; set; }
        public string MINIType { get; set; }
        public bool IsOWSettleAsBank { get; set; }
    }
}
