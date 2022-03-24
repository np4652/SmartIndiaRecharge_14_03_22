using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class EmpTarget:CommonReq
    {
        public int ID { get; set; }
        public int EmpID { get; set; }
        public int OID { get; set; }
        public int TargetTypeID { get; set; }
        public decimal Target{ get; set; }
        public decimal Commission { get; set; }
        public bool AmtType { get; set; }
        public bool IsEarned { get; set; }
        public bool IsGift { get; set; }
        public bool IsHikeOnEarned { get; set; }
        public decimal HikePer { get; set; }
        public string OName { get; set; }
        public decimal ChildTarget { get; set; }
        public string ImagePath { get; set; }
    }
}
