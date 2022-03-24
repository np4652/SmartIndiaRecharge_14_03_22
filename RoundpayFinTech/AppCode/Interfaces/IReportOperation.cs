using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IReportOperation
    {
        RechargeReportSummary GetTransactionSummary(int ServiceID);
        List<ProcRechargeReportResponse> GetRechargeReport(RechargeReportFilter filter);
        List<ProcAdminLedgerResponse> GetAdminLedgerList(ALedgerReportFilter filter);
    }
    
}
