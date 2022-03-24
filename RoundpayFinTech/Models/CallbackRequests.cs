namespace RoundpayFinTech.Models
{
    public class CallbackRequests
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public string EntryDate { get; set; }
        public string RequestIP { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }
    public class ROfferLog
    {
        public string Method { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string EntryDate { get; set; }
    }
    public class FetchBillLog
    {
        public string BillNo { get; set; }
        public string BillDate { get; set; }
        public string DueDate { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string EntryDate { get; set; }
        public string RequestURL { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }

    }
    public class APIUrlHittingLog
    {
        public string TransactionId { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string EntryDate { get; set; }
    }

    public class LookUpLog
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public string EntryDate { get; set; }
        public string LookUpNumber { get; set; }
        public string CurrentOperator { get; set; }
        public string CurrentCircle { get; set; }
    }

}
