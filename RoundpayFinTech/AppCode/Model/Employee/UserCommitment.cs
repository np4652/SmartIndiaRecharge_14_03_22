

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class UserCommitment
    {
        public int CommitmentID { get; set; }
        public string Prefix { get; set; }
        public int UserID { get; set; }
        public string UserMobile { get; set; }
        public string UserName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Role { get; set; }
        public int Commitment { get; set; }
        public int Achieved { get; set; }
        public int EmpID { get; set; }
        public int LoginTypeID { get; set; }
        public string EntryDate { get; set; }
        public string Longitute { get; set; }
        public string Latitude { get; set; }
        public decimal Balance { get; set; }
    }
    public class CommitmentSummary
    {
        public int TotalCommitment { get; set; }
        public int TotalAchieved { get; set; }
    }
    public class CommitmentSummaryChart
    {
        public string Service { get; set; }
        public decimal Amount { get; set; }
    }

}
