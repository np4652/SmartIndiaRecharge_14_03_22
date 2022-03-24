namespace Fintech.AppCode.Model
{
    public class APIURLHitting
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string TransactionID { get; set; }
        public string URL { get; set; }
        public string Response { get; set; }
        public string EntryDate { get; set; }
    }
}
