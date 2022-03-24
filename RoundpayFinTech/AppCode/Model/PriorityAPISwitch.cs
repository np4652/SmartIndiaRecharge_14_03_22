using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class PriorityApiSwitch
    {
        public int OID { get; set; }
        public int OpTypeID { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public string BlockAmount { get; set; }
        public List<APIDetail> APINameIDs { get; set; }
        public List<APISwitched> APISwitcheds { get; set; }
        public bool IsActive { get; set; }
        public int BackupAPIID { get; set; }
        public int BackupAPIIDRetailor { get; set; }
        public int RealAPIID { get; set; }
        public List<APIDetail> BackupAPIs { get; set; }
        public string Optional { get; set; }
        public List<PriorityApiSwitch> APIAmountSwitched { get; set; }
    }
    public class APISwitched
    {
        public int OID { get; set; }
        public int APIID { get; set; }
        public int BackupAPIIDRetailor { get; set; }
        public int RealAPIID { get; set; }
        public int MaxCount { get; set; }
        public int FailoverCount { get; set; }
        public bool IsActive { get; set; }
        public string Optional { get; set; }
        public int ModifyBy { get; set; }
        public string Amount { get; set; }
    }
    public class APISwitchedReq : CommonReq
    {
        public APISwitched aPISwitched { get; set; }
    }

    public class Userswitch
    {
        public int SwichID { get; set; }
        public int APIID { get; set; }
        public int OID { get; set; }
        public string APIName { get; set; }
        public string Operator { get; set; }
        public int UserID { get; set; }
        public string Prefix { get; set; }
        public string Role { get; set; }
        public string MobileNo { get; set; }
        public string OutletName { get; set; }
    }
}
