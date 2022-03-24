namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class BeneficiaryModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string SenderNo { get; set; }
        public string MobileNo { get; set; }
        public string IFSC { get; set; }
        public int BankID { get; set; }
        public string BankName { get; set; }
        public string APICode { get; set; }
        public string BeneID { get; set; }
        public int ID { get; set; }
        public int SelfRefID { get; set; }
        public string ReffID { get; set; }
        public int? TransMode { get; set; }
    }
}
