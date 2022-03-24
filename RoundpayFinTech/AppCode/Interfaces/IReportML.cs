using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IBillFetchReportML
    {
        Task<List<ProcBillFetchReportResponse>> GetBillFetchReport(RechargeReportFilter filter);
        Task<IEnumerable<ProcApiURlRequestResponse>> GetBillFetchApiResponses(string TID, string TransactionID);
    }

    public interface IReportML
    {
        IEnumerable<MasterPGateway> GetActivePaymentGateway();
        Task<List<ProcUserLedgerResponse>> GetUserPackageLimitLedgerrList(ULedgerReportFilter filter);
        IPStatusResp CheckIPGeoLocationInfo(IPGeoLocInfoReq iPGeoLocInfoReq);
        Task<ResponseStatus> AuthenticateAPIReqForIPGeoInfo(int userId, string token);
        Task<IPGeolocationInfo> GetIPGeolocationInfoByTable(string IP);
        Task<IPGeolocationInfo> GetIPGeolocationInfoByAPI(string IP, int UserId);
        Task<ResponseStatus> SaveIPGeolocationInfo(IPGeolocationInfo detail);
        #region FOSAccountStatement   
        List<ASAreaMaster> GetAreaFPC(int uid);
        ResponseStatus MapUserArea(CommonReq commonReq);
        ResponseStatus AreaMasterCU(ASAreaMaster aSAreaMaster);
        List<ASAreaMaster> GetAreaMaster(int uid);
        ResponseStatus ASPaymentCollection(ASCollectionReq aSCollectionReq);
        List<ASBanks> GetASBanks(int UID);
        List<AccountSummary> GetASCSummary(ULedgerReportFilter filter);
        AccountSummary GetASSummary(ULedgerReportFilter filter);
        Task<List<ProcUserLedgerResponse>> GetAccountStatement(ULedgerReportFilter filter);
        List<UserReport> GetUListForVoucherEntry(int UID);
        #endregion
        ResponseStatus GetQRStockDataByID(int RefID, int LT, int UID);
        ResponseStatus DownloadQR(int RefID);
        Task<ResponseStatus> BulkQRGeneration(int Qty);
        Task<QRGenerationReq> GetQRGenerationData(QRFilter qRFilter);
        Task<List<PendingRechargeNotification>> PendingRechargeNotification(bool isCallByAutoRply = false, int userId = 0, int LoginTypeId = 1);
        Task<List<PendingRechargeNotification>> PendingRefundNotification(bool isCallByAutoRply = false, int userId = 0, int LoginTypeId = 1, int APIId = 0);
        IEnumerable<OperatorParam> GetOperatorParam(int OID);

        HLRResponseStatus HitHLRAPIs(string ApiCode, string URL, int ApiId, int UserId, string MobNo);
        Task<List<MostUsedServices>> GetMostUsedServices();
        BCAgentSummary GetBCAgentSummary();
        Task<List<ProcRecentTransactionCounts>> GetMonthWeekDaysTransactions(string ActivityType, int RequestedDataType, int ServiceTypeID);
        Task<List<LoginDetail>> GetRecentLoginActivity(int TopRow);
        Task<List<ProcRecentTransactionCounts>> GetRecentTransactionActivity();
        Task<List<ProcRecentTransactionCounts>> GetTodayTransactionStatus(int RequestedDataType);
        Task<List<ProcRecentTransactionCounts>> GetDateOpTypeWiseTransactionStatus(string RequestedDate, int OpTypeID, int RequestedDataType);
        Task<List<ServiceMaster>> GetUsedServicesList();
        DealerSummary GetDealerSummary();
        AccountSummaryTable GetAccountSummaryTable();
        Task<List<ProcRecentTransactionCounts>> GetRecentDaysTransactions(int OpTypeID, int ServiceTypeID);
        Task<List<ProcRecentTransactionCounts>> GetRecentDaysPriSecTer(int PriSecTerType);

        Task<PSTReportUser> GetPriSecTerUserData();
        


        Task LoopRechTransactionStatus();
        Task LoopDMTTransactionStatus();
        void LoopAEPSTransactionStatus();
        Task<List<ProcRechargeReportResponse>> GetTopRecentTransactions(int OpTypeID, int Top);
        Task<List<ProcRechargeReportResponse>> GetTopRecentTransactions(int OpTypeID, int Top, int LoginID, int OutletID);
        Task<IEnumerable<ResponseStatus>> PendingTIDTransID(int RepType);
        List<ApiListModel> GetApiList();
        IEnumerable<Daybook> CircleIncentive(string FromDate, string ToDate, string Mob, int OID);
        List<WebNotification> WebNotificationsReport(CommonFilter Req);
        ResponseStatus DeactiveNotification(int ID, bool IsActive);
        Task markallread();

        IEnumerable<Daybook> DenominationTransactions(string FromDate, string ToDate, int UserID, int OID);
        List<SalesSummaryUserDateWise> GetUOpDateWiseTransaction(string FromDate, string ToDate, string UMobile);
        Task<List<UserActivityLog>> GetUserActivity(string FromDate, string ToDate, string MobileNo);
        Task<List<DTHSubscriptionReport>> GetDthsubscriptionPendings();
        Task<List<DTHSubscriptionReport>> GetDthsubscriptionReport(RechargeReportFilter filter);
        List<ResponseStatus> getRequestApproval(CommonReq req);
        IEnumerable<GetEditUser> GetUserBankUpdateRequest();
        IEnumerable<SattlementAccountModels> GetUserBankUpdateRequest1(SattlementAccountModels data);
        Task<IResponseStatus> AcceptOrRejectBankupdateRequest(GetEditUser RequestData);
        Task<List<ProcUserLedgerResponse>> GetUserLedgerList(ULedgerReportFilter filter);        
        List<ProcAdminLedgerResponse> GetAdminLedgerList(ALedgerReportFilter filter);        
        AccountSummary GetAccountSummary(ULedgerReportFilter filter);
        IEnumerable<CallbackRequests> CallbackReport(int Top, string SearchText);
        Task<PendingTransactios> PendingTransaction(int APIID, int OID, int RepType, string SenderNo = "");
        IResponseStatus UpdateRequestSent(int APIID);  
        List<ProcFundReceiveStatementResponse> GetUserFundReceive(ULFundReceiveReportFilter filter);        
        Task<RefundRequestResponse> MarkWrong2Right(WTRRequest _req);
        List<Daybook> AdminDayBook(string FromDate, string ToDate, int APIID, int OID, string Mobile_F);
        List<DMRDaybook> AdminDayBookDMR(string FromDate, string ToDate);
        List<Daybook> UserDaybook(string FromDate, string ToDate, string Mob);
        IEnumerable<Daybook> UserDaybookIncentive(string FromDate, string ToDate, string Mob, int OID);
        List<DMRDaybook> UserDaybookDMR(string FromDate, string ToDate, string Mob);
        List<SalesSummaryOpWise> GetSalesSummary(string FromDate, string ToDate, string UMobile);
        Task<List<SalesSummaryUserDateWise>> GetAPIUserSalesSummary(string FromDate, string ToDate, string UMobile);
        Task<List<SalesSummaryUserDateWise>> GetAPIUserSalesSummaryDate(string FromDate, string ToDate, string UMobile);
        Task<List<UserRolewiseTransaction>> GetUSalesSummary(string FromDate, string ToDate, int RoleID, int UserID);
        Task<List<CustomerCareActivity>> GetCCareActivity(string FromDate, string ToDate, string MobileNo, int OperationID);
        List<FundRequetResp> GetUserFundReport(FundOrderFilter fundOrderFilter);
        IEnumerable<FundRequetResp> GetUserFundReportApproval();
        ProcFundReceiveInvoiceResponse GetUserFundReceiveInvoice(string TransactionID);
        IEnumerable<SentSmsResponse> GetSentSMSreport(SentSMSRequest req);
        NumberSeriesListWithCircle GetNumberSeriesListWithCircles(int _OID);
        ResponseStatus UpdateNumberSeries(int _OID, int _CircleID, string _Number, string _OldNumber, char _F, int _ID);
        IResponseStatus UpdateNumberSeriesHelper(int ID, string Number, int OID, int CircleID, char Flag);
        HLRResponseStatus CheckNumberSeriesExist(string _Number);
        List<Daybook> AdminDayBookDaywise(string FromDate, string ToDate);
        Task<List<UserRolewiseTransaction>> GetUSalesSummaryDate(string FromDate, string ToDate, int RoleID, int UserID, bool IsSelf);
        Task<List<UserRolewiseTransaction>> GetUSalesSummaryOperator(string FromDate, string ToDate, int UserID, int OID, int OpTypeID);
        Task<List<UserRolewiseTransaction>> GetUSalesSummaryOperator(string FromDate, string ToDate, int UserID, int OID, int OpTypeID, bool IsDate);
        List<AdminSettlement> GetAdminSettlement(string FromDate, string ToDate, int WalletTypeID);
        IEnumerable<APIDaybookDatewise> AdminDayBookDateAPIwise(string FromDate, string ToDate, int APIID);
        IEnumerable<APIDaybookDatewise> AdminDayBookDateAPIwiseNew(string FromDate, string ToDate, int APIID);
        IEnumerable<WalletType> GetWalletTypes();
        Task<PendingTransactios> PESTransaction(int OID);
        Task<IEnumerable<PESReportViewModel>> GetPESDetails(int TID, string TransactionID);
        AccountSummary GetAccountSummaryDashboard();
        object GetLogDetails(CommonReq req);
        DataTable GetBeneficieryList(WalletRequest req);
        DataTable GetSettlementExcel(WalletRequest req);
        List<WalletRequest> GetWalletRequestReport(WalletRequest req);
        List<RefundTransaction> GetWTRLog(RefundLogFilter filter);
        IResponseStatus UpdateWTRStatus(RefundRequestData refundRequestData);
        IEnumerable<Dashboard_Chart> GetTodaySummaryChart();
        IEnumerable<Users> _GetUserList(CommonReq req);
        IResponseStatus UpdateUser(int UserID);
        IEnumerable<TopPerformer> GetTopPerformer(CommonReq req);
        Task<List<TransactionPG>> GetPGTransactionReport(TransactionPG filter);
        TransactionPGLogDetail GetTransactionPGLog(TransactionPGLogDetail param);
        Task<List<TransactionPG>> GetPendingPGTransactionReport();
        IResponseStatus ChangeTransactionPGStatus(TransactionPG param);
        Task<List<UserSettlement>> GetUserSettlement(SettlementFilter filter);
        Task<List<PSTReport>> GetPrimarySecoundaryReport(CommonFilter filter);
        List<PSTReport> PSTDeatilReport(CommonFilter filter);
        IResponseStatus UpdateBookingStatus(_CallbackData callbackData);
        Task<List<WebNotification>> GetWebNotification(bool IsShowAll);
        ResponseStatus CloseNotification(int id, int userID, string EntryDate, string bulkNotification = "");
        AccountSummary AdminAccountSummary();
        List<DashboardData> TotalBalRoleWise();
        int FundCount(int userID);
        Dashboard_Chart TotalPending();
        Task<List<MasterPaymentGateway>> GetMasterPaymentGateway(int id);
        AgreementApprovalNotification GetAgreementApprovalNotification(int UserID);
        IResponseStatus UpdateMasterPaymentGateway(MasterPaymentGateway AddData);
        IResponseStatus UpdatePaymentGateway(PaymentGateway AddData);
        Task<List<PaymentGateway>> GetPaymentGateway(CommonReq filter);
        Task<BillFetchSummary> GetBillFetchSummary(int UserID);
    }
    public interface IRechargeReportML {
        Task<IResponseStatus> CheckAEPSStatusAsync(int TID, string TransactionID = "");
        List<PESApprovedDocument> GetPESApprovedDocument(int id);
        Task<IResponseStatus> ResendTransactionAsync(int TID, int APIID);
        Task<List<ProcRechargeReportResponse>> GetRechargeReport(RechargeReportFilter filter);
        Task<List<ProcRechargeReportResponse>> GetDisplayLive(RechargeReportFilter filter);
        Task<List<RechargeReportResponse>> GetRechargeReportRole(RechargeReportFilter filter);
        Task<RechargeReportSummary> GetTransactionSummary(int ServiceID);
        Task<RechargeReportSummary> GetHotelSummary(int ServiceID);
        Task<IEnumerable<ProcApiURlRequestResponse>> GetRechargeApiResponses(int TID, string TransactionID);
        Task<IResponseStatus> UpdateTransactionStatus(_CallbackData callbackData);
        Task<IResponseStatus> CheckStatusAsync(int TID, string TransactionID = "");
        Task<IResponseStatus> ResendTransactionsAsync(string TIDs, int APIID);
        Task<IResponseStatus> ResendTransactionsAsync(string TIDs);
        Task<RefundRequestResponse> MarkDispute(RefundRequest _req);
        Task<IEnumerable<ProcRechargeReportResponse>> GetUplineCommission(int TID, string TransactionID);
        Task<List<ProcRechargeReportResponse>> GetRechargeFailToSuccess(RechargeReportFilter filter);
        Task<List<ProcRechargeReportResponse>> GetAEPSReport(RechargeReportFilter filter);
        Task<RefundRequestResponse> MarkWrong2Right(WTRRequest _req);
    }
    public interface IAppReportML
    {
        ResponseStatus MapQRToUser(string QRIntent, int UserID, int LT);

        #region FOSAccountStatement
        List<AccountSummary> GetASCSummary(ULedgerReportFilter filter);
        List<ASBanks> GetASBanks(int UID);
        ResponseStatus ASPaymentCollection(ASCollectionReq aSCollectionReq);
        Task<List<ProcUserLedgerResponse>> AppGetAccountStatement(ULedgerReportFilter filter);
        #endregion
        Task<List<DTHSubscriptionReport>> GetDthsubscriptionReport(_RechargeReportFilter _filter);
        Task<List<ProcRechargeReportResponse>> GetAppRechargeReport(_RechargeReportFilter _filter);
        Task<IEnumerable<ProcUserLedgerResponse>> GetAppUserLedgerList(ProcUserLedgerRequest filter);
        Task<IEnumerable<ProcFundReceiveStatementResponse>> GetAppUserFundReceive(ULFundReceiveReportFilter filter);
        IEnumerable<FundRequetResp> GetUserFundReportApp(FundOrderFilter fundOrderFilter);
        Task<List<ProcDMRTransactionResponse>> GetAppDMRReport(_RechargeReportFilter _filter);
        IEnumerable<FundRequetResp> GetUserFundReportApprovalApp(CommonReq commonReq);
        IEnumerable<AdminTransactionSummary> GetUSalesSummaryByAdmin(string FromDate, string ToDate);
        IEnumerable<Daybook> UserDaybookApp(CommonReq commonReq);
        IEnumerable<DMRDaybook> UserDaybookDMRApp(CommonReq commonReq);
        IEnumerable<WalletType> GetWalletTypes();
        IEnumerable<RefundTransaction> GetRefundLogApp(RefundLogFilter filter);
        Task<RefundRequestResponse> MarkDisputeApp(RefundRequestReq refundRequestReq);
        Task<RefundRequestResponse> MarkDispute(RefundRequestReq refundRequestReq);
        HLRResponseStatus CheckNumberSeriesExist(CommonReq req);
        Task<RefundRequestResponse> MarkWrong2RightApp(WTRRequest _req);
        List<RefundTransaction> GetWTRLogApp(_RefundLogFilter _filter);
        Task<IEnumerable<TargetAchieved>> GetTargetAchieveds(CommonReq commonReq);
        Task<IEnumerable<ProcRechargeReportResponse>> GetAEPSReport(_RechargeReportFilter _filter);
        List<WalletRequest> appGetWalletRequestReport(WalletRequest req);
    }
    public interface IDMRReportML {
        Task<List<ProcDMRTransactionResponse>> GetDMRReport(RechargeReportFilter filter);
        Task<List<DMRReportResponse>> GetDMRReportRole(RechargeReportFilter filter);
        Task<IResponseStatus> CheckStatusDMRAsync(int TID, string TransactionID,bool FromBulk = false);
    }
    public interface IRefundReportML {
        RefundAPITransactios GetRefundLog(RefundLogFilter filter);

        Task<IResponseStatus> AcceptOrRejectRefundRequest(RefundRequestData refundRequestData);
    }
    public interface IOutletML
    {
        Task<List<OutletsOfUsersList>> GetRecentOutletUserList(int TopRow);
        Task<List<MAtmModel>> GetmAtmRequestList(MAtmFilterModel filter);
        Task<mAtmRequestResponse> UpdatemAtmRequestList(int id, int status);
        Task<List<OutletsOfUsersList>> GetOutletUserList(OuletOfUsersListFilter filter);
        OutletsOfUsersList GetOutletUserList(OuletOfUsersListFilter filter, int LoginID);
        Task<IResponseStatus> UploadOutletUsersExcel(OutletsReqData ReqData);
        IEnumerable<ApiWiseDetail> GetApiWiseDetail(int OutletID);
    }
    public interface IInvoiceReportML
    {
        InvoiceResponseModel GetInvoiceData(string Mobile, string Month, int InvoiceType);
        IEnumerable<CalculatedGSTEntry> GetGSTSummary(GSTReportFilter gSTReportFilter);
        IEnumerable<CalculatedGSTEntry> GetTDSSummary(GSTReportFilter gSTReportFilter);
        IEnumerable<InvoiceDetail> GetInvoiceMonths(string Mobile);
        List<InvoiceSettings> InvoiceSettings(bool IsDisable, int ID, int OpsCode);
        List<P2AInvoiceListModel> P2AInvoiceApprovalList(string Mobile);
        InvoiceResponseModel GetInvoiceSummary(string Mobile, string Month);
    }
}
