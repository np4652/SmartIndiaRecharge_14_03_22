namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcFundReceiveStatementResponse
    {
        public int StatusCode { get; set; }
        public string Description { get; set; }
        public string TransactionID { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string EntryDate { get; set; }
        public string Amount { get; set; }
        public string CurrentAmount { get; set; }
        public string Remark { get; set; }
        public int ServiceTypeID { get; set; }
        public int WalletID { get; set; }
        public string OtherUser { get; set; }
    }
    public class ProcFundReceiveInvoiceResponse
    {
        public int ResultCode { get; set; }
        public string Msg { get; set; }
        public string TID { get; set; }
        public string EntryDate { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string PanNo { get; set; }
        public string Address { get; set; }
        public string Amount { get; set; }
        public string EmailId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string MobileNo { get; set; }

        public string CName { get; set; }        public string CAddress { get; set; }        public string CMobileNo { get; set; }        public string CPhoneNo { get; set; }        public string CWhatsapp { get; set; }        public string CEmail { get; set; }
    }
}
