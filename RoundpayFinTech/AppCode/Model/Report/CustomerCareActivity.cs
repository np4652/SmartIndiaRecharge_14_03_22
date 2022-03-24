namespace Fintech.AppCode.Model.Reports
{
    public class CustomerCareActivity
    {
        public int ID { get; set; }
        public string Customercare { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public string Activity { get; set; }
        public string OperationName { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public string EntryDate { get; set; }
    }
    public class UserActivityLog
    {
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Action { get; set; }
        public string EntryDate { get; set; }
    }
}
