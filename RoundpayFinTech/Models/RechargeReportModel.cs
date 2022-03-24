using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.Models
{
    public class ReportModelCommon {
        public bool IsWLAPIAllowed { get; set; }
        public bool IsEndUser { get; set; }
        public bool CanSuccess { get; set; }
        public bool CanFail { get; set; }
        public bool CanMarkDispute { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAPIUser { get; set; }
        public bool IsEmployee { get; set; }
        public IEnumerable<APIDetail> RechargeAPI { get; set; }
        public IEnumerable<OperatorDetail> Operators { get; set; }
        public IEnumerable<OpTypeMaster> OpTypes { get; set; }
        public SelectList Roles { get; set; }
    }
    public class ShowIPAddressModel : ReportModelCommon {
        public IEnumerable<IPAddressModel> IPAddressModels { get; set; }
    }
    public class RechargeReportModel: ReportModelCommon
    {
        public int Flag { get; set; }
        public IEnumerable<ProcRechargeReportResponse> Report { get; set; }
    }
    public class PendingTransactionModel : ReportModelCommon {
        public PendingTransactios Report { get; set; }
    }
    public class DMRReportModel: ReportModelCommon
    {
        public IEnumerable<ProcDMRTransactionResponse> Report { get; set; }
        public List<BankMaster> Banks { get; set; }
    }
    public class SalesSummaryModel : ReportModelCommon
    {
        public List<SalesSummaryOpWise> Report { get; set; }
        public List<SalesSummaryUserDateWise> salesSummaryRoleDateWises { get; set; }
        public List<UserRolewiseTransaction> userRolewiseTransactions { get; set; }
        public string UserMobile { get; set; }
        public int UserID { get; set; }
    }
    public class LedgerReportModel
    {
        public bool IsAdmin { get; set; }
        public bool IsEmployee { get; set; }
        public bool HaveChild { get; set; }
        public string SelfMobile { get; set; }
        public UserBalnace userBalnace { get; set; }
    }
    public class BillFetchReportModel : ReportModelCommon
    {
        public BillFetchSummary BillFetchSummary { get; set; }
        public IEnumerable<ProcBillFetchReportResponse> Report { get; set; }
    }

    public class BillFetchSummary : ReportModelCommon
    {

        public int TotalFetchBill { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPaid { get; set; }

    }
}
