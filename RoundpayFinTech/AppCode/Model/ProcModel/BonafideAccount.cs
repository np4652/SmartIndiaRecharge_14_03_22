using Fintech.AppCode.Model.Reports;
using System.Collections.Generic;


namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class BonafideAccount
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string AccountNo { get; set; }
        public string IFSC { get; set; }
        public string PyeeName { get; set; }
        public bool IsActive { get; set; }
        public string ModifyDate { get; set; }
        public int LoginID { get; set; }
        public int LTID { get; set; }
        public bool IsDelete { get; set; }
        public int UPICount { get; set; }
    }
    public class Bonafide : BonafideAccount
    {
        public List<BonafideAccount> BonafideAccountList { get; set; }
        
    }
    
}