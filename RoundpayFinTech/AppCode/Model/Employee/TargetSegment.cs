
namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class TargetSegment
    {
        public string Type { get; set; }
        public decimal Target{ get; set; }
        public decimal Achieve{ get; set; }
        public decimal AchievePercent{ get; set; }
        public decimal Incentive{ get; set; }
        public string strIncentive { get; set; }
    }
}
