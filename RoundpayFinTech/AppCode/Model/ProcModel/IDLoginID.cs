using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class IDLoginID
    {
        public int LoginID { get; set; }
        public int ID { get; set; }
        public int ID2 { get; set; }
        public string IDs { get; set; }
        public bool Status { get; set; }
        public decimal ChargedAmount { get; set; }
        public string AccountNo { get; set; }
        public string OTP { get; set; }
        public int ServiceTypeID { get; set; }
        public string AccountNo2 { get; set; }
        public char CharParam { get; set; }
    }

    public class InvoiceDetail
    {
        public int InvoiceID { get; set; }
        public int UserID { get; set; }
        public string InvoiceMonth { get; set; }
        public int UploadStatus { get; set; }
        public string InvoiceURL { get; set; }
        public string UploadedDate { get; set; }
        public string Remark { get; set; }
        public decimal GSTAmount { get; set; }
        public string GSTIN { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
    }
    public class P2AInvoiceListModel
    {
        public int UserID { get; set; }
        public string MobileNo{ get; set; }
        public string OutletName{ get; set; }
        public string Name{ get; set; }
        public string EmailID{ get; set; }
        public string GSTIN{ get; set; }
        public string PAN{ get; set; }
        public string GSTMonth { get; set; }
        public decimal GSTAmount{ get; set; }
        public string P2AInvoiceURL { get; set; }
    }
    public class InvoiceSettings
    {
        public int ID { get; set; }
        public string InvoiceMonth { get; set; }
        public bool IsDisable { get; set; }
        public string _EntryDate { get; set; }
        public string _ModifyDate { get; set; }
        public int _ModifyBy { get; set; }
        public int StatusCode { get; set; }
        public string Msg { get; set; }
    }
    public class InvoiceListModel
    {
        public bool IsAdmin { get; set; }
        public string MobileNo { get; set; }
        public IEnumerable<InvoiceDetail> invoiceDetails { get; set; }
    }
}
