using Fintech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class Circle
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int OID { get; set; }
        public int APIID { get; set; }
        public bool IsActive { get; set; }
        public int EntryBy { get; set; }
        public int MaxCount { get; set; }
    }
    public class CircleAPISwitchDetail
    {
        public int OID { get; set; }
        public int APIID { get; set; }
        public string APIName { get; set; }
        public string Operator { get; set; }
        public bool IsSwitched { get; set; }
        public int MaxCount { get; set; }
    }
    public class CircleSwitch
    {
        public int OID { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public List<Circle> CircleSwitched { get; set; }
        public List<Circle> Circles { get; set; }
    }
    public class CircleReq : CommonReq {
        public Circle circle { get; set; }
    }
}
