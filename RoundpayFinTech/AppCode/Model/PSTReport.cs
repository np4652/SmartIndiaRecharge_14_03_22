namespace RoundpayFinTech.AppCode.Model
{
    public class PSTReport:ResponseStatus
    {
        
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public int RoleID { get; set; }
        public string Prefix { get; set; }
        public double PriAmount { get; set; }
        public double SecAmount { get; set; }
        public double Recharge { get; set; }
        public double MoneyTransfer { get; set; }
        public double BillPayment { get; set; }
        public double AEPS { get; set; }
        public double GenralInsurance { get; set; }
        public double Shopping { get; set; }
        public double EServices { get; set; }
        public double PSAService { get; set; }
        public double DTHSubscription { get; set; }
        public int ServiceID { get; set; }
        public string Service { get; set; }
        public string TransactionDate { get; set; }
        
        public double Amount { get; set; }
    }
}
