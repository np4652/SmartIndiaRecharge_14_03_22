using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class InvoiceResponse
    {
        public int InvoiceRefID { get; set; }
        public int UserID { get; set; }
        public string MobileNo { get; set; }
        public string OutletName { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string GSTIN { get; set; }
        public string PAN { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public string Operator { get; set; }
        public string HSNCode { get; set; }
        public string State { get; set; }
        public decimal Requested { get; set; }
        public decimal Amount { get; set; }
        public decimal CommAmount { get; set; }
        public decimal NetAmount { get; set; }
        public int WID { get; set; }
    }
    public class InvoiceSummaryResponse
    {
        public string BillingModel { get; set; }
        public int OpTypeID { get; set; }
        public string OpType { get; set; }
        public decimal Requested { get; set; }
        public decimal Amount { get; set; }
        public decimal CommAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal GSTTaxAmount { get; set; }
    }
    public class InvoiceResponseModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public InvoiceCompanyDetail CompanyDetail { get; set; }
        public int WID { get; set; }
        public int UserID { get; set; }
        public string UserAddress { get; set; }
        public string UserState { get; set; }
        public string UserEmailID { get; set; }
        public string UserOutletName { get; set; }
        public string UserPAN { get; set; }
        public string UserGST { get; set; }
        public string InvoiceMonth { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int InvoiceNo { get; set; }
        public IEnumerable<InvoiceResponse> InvoiceList { get; set; }
        public List<InvoiceSummaryResponse> invoiceSummaries { get; set; }
    }

    public class InvoiceCompanyDetail
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyGST { get; set; }
        public string CompanyMobile { get; set; }
        public string CompanyMobile2 { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyState { get; set; }
        public string CompanyPincode { get; set; }
        public string CompanyPAN { get; set; }
        public string InvoicePrefix { get; set; }
    }
    public class CalculatedGSTEntry
    {
        public int InvoiceID { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public string State { get; set; }
        public string PAN { get; set; }
        public string GSTIN { get; set; }
        public bool IsGSTVerified { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal GSTTaxAmount { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string CompanyState { get; set; }
        public string InvoiceMonth { get; set; }
        public string BillingModel { get; set; }
        public bool IsHoldGST { get; set; }
        public bool ByAdminUser { get; set; }
    }
    public class GSTReportFilter
    {
        public int LoginTypeID { get; set; }
        public int LoginID { get; set; }
        public string MobileNo { get; set; }
        public string GSTMonth { get; set; }
        public int IsGSTVerified { get; set; }
        public string BillingModel { get; set; }
    }
}
