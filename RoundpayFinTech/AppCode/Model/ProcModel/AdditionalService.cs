using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{

    public class ActAddonSerReq
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public int OpTypeID { get; set; }
        public int OID { get; set; }
        public int OutletID { get; set; }
        public int UID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }

    public class AddonServ
    {
        public int OutletID { get; set; }
        public int UID { get; set; }
        public int OpTypeID { get; set; }
        public int OID { get; set; }
        public int IDLimit { get; set; }
        public string  DisplayName { get; set; }
        public bool IsActive { get; set; }
        public int ServiceChargeDeuctionType { get; set; }
        public bool IsIDLimitByOpertor { get; set; }   
    }

    public class GetAddService : ResponseStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<AddonServ> AddonServList { get; set; }
        public DataTable dt { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }

    }

}
