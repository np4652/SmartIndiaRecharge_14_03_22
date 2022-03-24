using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using RoundpayFinTech.Models;
using System.Collections.Generic;


namespace RoundpayFinTech.AppCode.Model.App
{
    public class AppConst
    {
        public const string APPID = "ROUNDPAYAPPID13APR20191351";
        public const string Version = "1.0";
        public const string WEBAPPID = "6072874e1f4b7000991915fa914318ed";
        public const string PlayStoreRefLink = "https://play.google.com/store/apps/details?id=com.solution.dap.pro&referrer={USER_ID}";
    }
    public class AppRequest
    {
        public string APPID { get; set; }
        public string IMEI { get; set; }
        public string RegKey { get; set; }
        public string Version { get; set; }
        public string SerialNo { get; set; }
        public int LoginTypeID { get; set; }
    }
    public class GetStatesReq : AppSessionReq
    {
        public bool IsB2C { get; set; }
    }
    public class FlagChecksResponse : AppResponse
    {
        public bool IsEmailVerified { get; set; }
        public bool IsSocialAlert { get; set; }
    }
    public class SocialAlertSettingRequest : AppSessionReq
    {
        public string WhatsappNo { get; set; }
        public string TelegramNo { get; set; }
        public string HangoutId { get; set; }
    }
    public class AppSessionReq : AppRequest
    {
        public int UserID { get; set; }
        public int SessionID { get; set; }
        public string Session { get; set; }
        public string SecurityKey { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
        public int OID { get; set; }
    }
    public class ProceedToPayReq : AppSessionReq
    {
        public bool IsB2C { get; set; }

    }
    public class GetShippingAddressesReq : AppSessionReq
    {
        public bool IsB2C { get; set; }

    }
    public class AppEKYCEditRequest: AppSessionReq
    {
        public int BankID { get; set; }
        public string IFSC { get; set; }
        public string OTP { get; set; }
        public int ReffID { get; set; }
        public int StepID { get; set; }
        public string Director { get; set; }
        public string VerificationAccount { get; set; }
        public bool IsConcent { get; set; }
        public bool IsSkip { get; set; }
        public int CompanyTypeID { get; set; }
    }
    public class AppSettlementAccountUpdateReq: AppSessionReq
    {
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public int BankID { get; set; }
        public int UpdateID { get; set; }
        public string UTR { get; set; }
    }
    public class AppCommonRequest : AppSessionReq
    {
        public int OType { get; set; }
    }
    public class AppPGChooseReq : AppSessionReq
    {
        public bool IsUPI { get; set; }
    }
    public class AppOnboardCheckReq : AppSessionReq
    {
        public int OutletID { get; set; }
        public string OTP { get; set; }
        public string PidData { get; set; }
        public int OTPRefID { get; set; }
        public bool IsBio { get; set; }
        public int BioAuthType { get; set; }
    }
    public class AppRequestReferral : AppRequest
    {
        public int ReferralID { get; set; }
    }
    public class AppLogoutRequest : AppSessionReq
    {
        public int SessType { get; set; }
    }
    public class AppFCMIDRequest : AppSessionReq
    {
        public string FCMID { get; set; }
    }
    public class AppNewsRequest : AppSessionReq
    {
        public bool IsLoginNews { get; set; }
    }
    public class OIDSessonRequest : AppSessionReq
    {
    }
    public class PGInitiatePGRequest : AppSessionReq
    {
        public int Amount { get; set; }
        public int UPGID { get; set; }
        public int WalletID { get; set; }
    }
    public class UPIInitiateRequest : AppSessionReq
    {
        public int WalletID { get; set; }
        public int Amount { get; set; }
        public string UPIID { get; set; }
    }
    public class PGUpdate : AppSessionReq
    {
        public int TID { get; set; }
        public int Amount { get; set; }
        public string Hash { get; set; }
        public PaytmPGResponse PaytmCallbackResp { get; set; }
    }
    public class UPIUpdate : AppSessionReq
    {
        public string TID { get; set; }
        public string BankStatus { get; set; }
    }

    public class AEPSBalanceRequest : AppSessionReq
    {
        public PidData pidData { get; set; }
        public string pidDataXML { get; set; }
        public string Aadhar { get; set; }
        public string BankName { get; set; }
        public string BankIIN { get; set; }
        public int InterfaceType { get; set; }
        public int Amount { get; set; }
        public string Reff1 { get; set; }
        public string Reff2 { get; set; }
        public string Reff3 { get; set; }
        public string OTP { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }

    }
    public class MiniBankIntiateRequest : AppSessionReq
    {
        public int SDKType { get; set; }
        public int Amount { get; set; }
    }
    public class AppMinBankupdateRequest : AppSessionReq
    {
        public int TID { get; set; }
        public string VendorID { get; set; }
        public int APIStatus { get; set; }
        public string Remark { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
    }
    public class AppAutoBillModel : AppSessionReq
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int UserIdInput { get; set; }
        public bool IsAutoBilling { get; set; }
        public int MaxBillingCountAB { get; set; }
        public int BalanceForAB { get; set; }
        public int AlertBalance { get; set; }
        public bool FromFOSAB { get; set; }
        public int MaxCreditLimitAB { get; set; }
        public int MaxTransferLimitAB { get; set; }
    }

    public class AppResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public bool IsVersionValid { get; set; }
        public bool IsAppValid { get; set; }
        public int CheckID { get; set; }
        public bool IsPasswordExpired { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public bool IsLookUpFromAPI { get; set; }
        public bool IsDTHInfoCall { get; set; }
        public bool IsShowPDFPlan { get; set; }
        public string SID { get; set; }
        public string PCode { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsBioMetricRequired { get; set; }
        public int BioAuthType { get; set; }
        public bool IsRedirectToExternal { get; set; }
        public string ExternalURL { get; set; }
        public bool IsResendAvailable { get; set; }
        public int GetID { get; set; }
        public bool IsDTHInfo { get; set; }
        public bool IsRoffer { get; set; }
        public bool IsGreen { get; set; }
        public int RID { get; set; }
        public bool IsPaymentGateway { get; set; }
        public string B2CDomain { get; set; }
        public bool ShouldSerializeGetID() => false;
        public bool ShouldSerializeB2CDomain() => false;
        public bool ShouldSerializeIsPaymentGateway() => false;
        //public bool ShouldSerializeRID() => false;
        public bool IsBulkQRGeneration { get; set; }
        public bool IsSattlemntAccountVerify { get; set; }
        public bool IsEKYCForced { get; set; }
        public bool IsCoin { get; set; }

    }
    public class AppResponseData : AppResponse
    {
        public object data { get; set; }
        public bool IsSattlemntAccountVerify { get; set; }
    }
    public class MiniBankStatusResponse : AppResponse
    {
        public MBStatuscheckResponseApp data { get; set; }
    }
    public class AppDoubleFactorResp : AppResponse
    {
        public string RefID { get; set; }
    }
    public class AppTransactionReq : AppSessionReq
    {
        public string AccountNo { get; set; }
        public decimal Amount { get; set; }
        public string O1 { get; set; }
        public string O2 { get; set; }
        public string O3 { get; set; }
        public string O4 { get; set; }
        public string CustomerNo { get; set; }
        public string RefID { get; set; }
        public string GeoCode { get; set; }
        public int OutletID { get; set; }
        public bool IsReal { get; set; }
        public int PromoCodeID { get; set; }
        public int FetchBillID { get; set; }
    }
    public class AppDTHSubscriptionReq : AppSessionReq
    {
        public int PID { get; set; }
        public string Customer { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public int AreaID { get; set; }
        public string CustomerNumber { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string SecurityKey { get; set; }
    }

    public class AppTransactionRes : AppResponse
    {
        public string LiveID { get; set; }
        public string TransactionID { get; set; }
    }
    public class AEPSBanksResponse : AppResponse
    {
        public IEnumerable<BankMaster> aepsBanks { get; set; }
    }
    public class AEPSBalanceResponse : AppResponse
    {
        public double Balance { get; set; }
    }
    public class AppResponseForUPIOrder : AppResponse
    {
        public int OrderID { get; set; }
    }
    public class VirtualAccountResponse : AppResponse
    {
        public UserQRInfo userQRInfo { get; set; }
    }
    public class AEPSWithResponse : AppResponse
    {
        public int Status { get; set; }
        public string LiveID { get; set; }
        public string TransactionID { get; set; }
        public string ServerDate { get; set; }
        public string BeneficiaryName { get; set; }
        public double Balance { get; set; }
    }
    public class AEPSDepositOTPResponse : AppResponse
    {
        public string Reff1 { get; set; }
        public string Reff2 { get; set; }
        public string Reff3 { get; set; }
    }
    public class BankMiniSTMTResponse : AppResponse
    {
        public string Balance { get; set; }
        public List<MiniStatement> Statements { get; set; }
    }
    public class MiniBankInitiateResponse : AppResponse
    {
        public int tid { get; set; }
    }
    public class AppBillfetchResponse : AppResponse
    {
        public BBPSResponse bBPSResponse { get; set; }
    }
    public class LoginRequest : AppRequest
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
    public class AppLoginResponse : AppResponse
    {
        public bool IsReferral { get; set; }
        public string OTPSession { get; set; }
        public LoginData Data { get; set; }
        public bool ShouldSerializeData() => (Statuscode == 1);
        public bool ShouldSerializeOTPSession() => (Statuscode == 2 || Statuscode == 3);
        public bool IsHeavyRefresh { get; set; }
        public bool IsTargetShow { get; set; }
        public bool IsRealAPIPerTransaction { get; set; }
        public bool IsAdminFlatComm { get; set; }
        public int ActiveFlatType { get; set; }
        public bool IsDenominationIncentive { get; set; }
        public bool IsAutoBilling { get; set; }
        public bool WithCustomLoginID { get; set; }
        public bool IsVirtualAccountInternal { get; set; }
        public bool IsAccountStatement { get; set; }
        public bool IsAreaMaster { get; set; }
        public bool IsPlanServiceUpdated { get; set; }
        public bool IsMultiLevel { get; set; }
    }
    public class LoginData
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public int SessionID { get; set; }
        public string RoleName { get; set; }
        public int RoleID { get; set; }
        public int SlabID { get; set; }
        public int LoginTypeID { get; set; }
        public string EmailID { get; set; }
        public string Session { get; set; }
        public int OutletID { get; set; }
        public bool IsDoubleFactor { get; set; }
        public bool IsPinRequired { get; set; }
        public bool IsRealAPI { get; set; }
        public bool IsDebitAllowed { get; set; }
        public bool IsMultiLevel { get; set; }
        public int StateID { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public int WID { get; set; }
        public string AccountSecretKey { get; set; }
    }
    public class OTPRequest : AppRequest
    {
        public string OTP { get; set; }
        public string OTPSession { get; set; }
        public int OTPType { get; set; }
        public string Domain { get; set; }
    }
    public class CallMeReqApp : AppSessionReq
    {
        public string mobileNo { get; set; }
    }
    public class BalanceResponse : AppResponse
    {
        public bool IsReferral { get; set; }
        public UserBalnace Data { get; set; }
        public bool IsMoveToPrepaid { get; set; }
        public bool IsMoveToUtility { get; set; }
        public bool IsMoveToBank { get; set; }
        public bool IsFlatCommission { get; set; }
        public int ActiveFlatType { get; set; }
        public bool IsWalletToWallet { get; set; }
    }
    public class CommRateResponse : AppResponse
    {
        public decimal CommRate { get; set; }
    }
    public class WalletResponse : AppResponse
    {
        public IEnumerable<WalletType> WalletTypes { get; set; }
        public List<MoveToWalletMapping> moveToWalletMappings { get; set; }
    }

    public class OperatorResponse : AppResponse
    {
        public bool IsHeavyRefresh { get; set; }
        public bool IsTakeCustomerNo { get; set; }
        public OpNumberData Data { get; set; }
    }
    public class AppServiceProviders : AppResponse
    {
        public bool IsHeavyRefresh { get; set; }
        public bool IsTakeCustomerNo { get; set; }
        public IEnumerable<OperatorDetail> Operators { get; set; }
    }
    public class AppNumberSeries : AppResponse
    {
        public IEnumerable<NumberSeries> NumSeries { get; set; }
    }
    public class AppCircles : AppResponse
    {
        public IEnumerable<CirlceMaster> Cirlces { get; set; }
    }
    public class OpNumberData
    {
        public IEnumerable<NumberSeries> NumSeries { get; set; }
        public IEnumerable<OperatorDetail> Operators { get; set; }
        public IEnumerable<CirlceMaster> Cirlces { get; set; }
    }
    public class IndustryTypeAppModel : AppResponse
    {
        public IEnumerable<IndustryTypeModel> data { get; set; }
    }
    public class ServicesAssigned : AppResponse
    {
        public Package_ClData Data { get; set; }
        public bool IsAddMoneyEnable { get; set; }
        public bool IsPaymentGatway { get; set; }
        public bool IsUPI { get; set; }
        public bool IsUPIQR { get; set; }
        public bool IsECollectEnable { get; set; }
        public bool IsDMTWithPipe { get; set; }
    }
    public class Package_ClData
    {
        public IEnumerable<Package_Cl> AssignedOpTypes { get; set; }
    }
    public class OperatoOptionalsResponse : AppResponse
    {
        public OperatorParamModels Data { get; set; }
    }
    public class OpOptionalData
    {
        public IEnumerable<OperatorOptional> OperatorOptionals { get; set; }
    }
    public class AppOnboardResponse : AppResponse
    {
        public bool IsConfirmation { get; set; }
        public bool IsRedirection { get; set; }
        public bool IsDown { get; set; }
        public bool IsWaiting { get; set; }
        public bool IsRejected { get; set; }
        public bool IsIncomplete { get; set; }
        public bool IsUnathorized { get; set; }
        public short SDKType { get; set; }
        public AppSDKDetail SDKDetail { get; set; }
        public List<_BCResponse> BCResponse { get; set; }
        public string PANID { get; set; }
        public string GIURL { get; set; }
        public bool IsShowMsg { get; set; }
        public bool InInterface { get; set; }
        public short InterfaceType { get; set; }
        public int OTPRefID { get; set; }
    }
    #region ReportRelated
    public class AppReportCommon : AppSessionReq
    {
        public string TransactionID { get; set; }
        public string AccountNo { get; set; }
        public string ChildMobile { get; set; }
        public int TopRows { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool IsExport { get; set; }
        public bool IsRecent { get; set; }
        public int Status { get; set; }
        public int Criteria { get; set; }
        public string CriteriaText { get; set; }
        public string TransMode { get; set; }
        public int BookingStatus { get; set; }
    }
    public class AppRechargeReportReq : AppReportCommon
    {
        public int OpTypeID { get; set; }
    }
    public class AppReceiptRequest : AppSessionReq
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public decimal ConvenientFee { get; set; }
    }
    public class AppTargetRequest : AppSessionReq
    {
        public bool IsTotal { get; set; }
    }
    public class AppRefundLogReq : AppReportCommon
    {
        public int DateType { get; set; }
    }
    public class AppRefundRequest : AppSessionReq
    {
        public int TID { get; set; }
        public string TransactionID { get; set; }
        public string OTP { get; set; }
        public bool IsResend { get; set; }
    }
    public class AppDMTReportReq : AppReportCommon
    {
        public string SenderMobile { get; set; }
    }
    public class AppLedgerReq : AppReportCommon
    {
        public int WalletTypeID { get; set; }
    }
    public class AppFundDCReq : AppReportCommon
    {
        public int ServiceID { get; set; }
        public bool IsSelf { get; set; }
        public int WalletTypeID { get; set; }
        public string OtherUserMob { get; set; }
    }
    public class AppFundOrderReportReq : AppReportCommon
    {
        public int TMode { get; set; }
        public bool IsSelf { get; set; }
        public string UMobile { get; set; }
    }
    public class AppMyCommReq : AppSessionReq
    {
        public int SlabID { get; set; }
        public decimal Amount { get; set; }
    }
    public class AppPopupResponse : AppResponse
    {
        public string Popup { get; set; }
    }
    public class AppReportResponse : AppResponse
    {
        public string RefferalContent { get; set; }
        public IEnumerable<BannerImage> RefferalImage { get; set; }
        public IEnumerable<ProcRechargeReportResponse> RechargeReport { get; set; }
        public IEnumerable<DTHSubscriptionReport> DTHSubscriptions { get; set; }
        public IEnumerable<ProcDMRTransactionResponse> DMTReport { get; set; }
        public IEnumerable<ProcUserLedgerResponse> LedgerReport { get; set; }
        public IEnumerable<Daybook> UserDaybookReport { get; set; }
        public IEnumerable<DMRDaybook> UserDaybookDMRReport { get; set; }
        public IEnumerable<ProcFundReceiveStatementResponse> FundDCReport { get; set; }
        public IEnumerable<FundRequetResp> FundOrderReport { get; set; }
        public IEnumerable<SlabCommission> SlabCommissions { get; set; }
        public IEnumerable<SlabDetailDisplayLvl> SlabDetailDisplayLvl { get; set; }
        public IEnumerable<UserReportBulk> UserList { get; set; }
        public IEnumerable<RoleMaster> ChildRoles { get; set; }
        public IEnumerable<FundRequetResp> FundRequestForApproval { get; set; }
        public News NewsContent { get; set; }
        public IEnumerable<BannerImage> Banners { get; set; }
        public string AppLogoUrl { get; set; }
        public IEnumerable<Notification> Notifications { get; set; }
        public IEnumerable<RefundTransaction> RefundLog { get; set; }
        public CompanyProfileDetail CompanyProfile { get; set; }
        public IEnumerable<TargetAchieved> TargetAchieveds { get; set; }
        public IEnumerable<ProcRechargeReportResponse> AEPsDetail { get; set; }
        public IEnumerable<IncentiveDetail> IncentiveDetails { get; set; }
        public IEnumerable<WalletRequest> MoveToWalletReport { get; set; }
        public UserList FOSList { get; set; }
        public IEnumerable<Video> VideoList { get; set; }
        public IEnumerable<AccountSummary> ASCReport { get; set; }
        public TransactionDetail receiptDetail { get; set; }
        public bool IsDrawOpImage { get; set; }
        public IEnumerable<AccountOpData> accountOpeningDeatils { get; set; }
        public List<ProcUserLedgerResponse> accountStatementSummary { get; set; }
    }
    public class AppObjectResponse : AppResponse
    {
        public SlabCommission slabDetail { get; set; }
        public CommissionDisplay commissionDisplay { get; set; }
    }
    public class AppUserRefferalDetail : AppResponse
    {
        public UserRegModel userRegModel { get; set; }
    }
    public class AppPaymentModeForPGRes : AppResponse
    {
        public IEnumerable<OperatorDetail> PaymentModes { get; set; }
    }
    public class AppPaymentGatewayType : AppResponse
    {
        public IEnumerable<PaymentGatewayModel> PGs { get; set; }
        public IEnumerable<PaymentGatewayDetail> PGForUPI { get; set; }

    }
    public class PGInitiatePGResponse : AppResponse
    {
        public PGModelForApp pGModelForApp { get; set; }
        public PGModelForRedirection pGModelForWeb { get; set; }
        public CollectUPPayResponse pGModelforUpi { get; set; }

        public int Status { get; set; }
    }
    public class UPIIntiateResponse : AppResponse
    {
        public string TID { get; set; }
        public string BankOrderID { get; set; }
        public string MVPA { get; set; }
        public string TerminalID { get; set; }
    }
    public class AppUserDocuments : AppResponse
    {
        public IEnumerable<DocTypeMaster> UserDox { get; set; }
    }
    public class AppUserProfile : AppResponse
    {
        public GetEditUser UserInfo { get; set; }
    }
    public class WebMemberTypeModel : AppResponse
    {
        public IEnumerable<MembershipmasterB2C> MemberTypes { get; set; }
    }
    public class WebAppUserProfileResp : AppResponse
    {
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string EmailID { get; set; }
        public string AlternateMobile { get; set; }
        public string DOB { get; set; }
        public string PAN { get; set; }
        public string Pincode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string Aadhar { get; set; }
        public string ProfilePic { get; set; }


    }
    #endregion
    #region FundRequestRelated
    public class AppFundRequestToResponse : AppResponse
    {
        public IEnumerable<FundRequestToUser> FundRequestToUsers { get; set; }
    }
    public class AppGetBankRequest : AppSessionReq
    {
        public int ParentID { get; set; }
    }
    public class AppBankResponse : AppResponse
    {
        public IEnumerable<Bank> Banks { get; set; }
        public IEnumerable<PaymentModeMaster> PaymentModes { get; set; }
        public IEnumerable<BankMaster> BankMasters { get; set; }
    }
    public class AppFundRequest : AppSessionReq
    {
        public int BankID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionID { get; set; }
        public string MobileNo { get; set; }
        public string ChequeNo { get; set; }
        public string CardNo { get; set; }
        public string AccountHolderName { get; set; }
        public int PaymentID { get; set; }
        public int WalletTypeID { get; set; }
        public string UPIID { get; set; }
        public string Branch { get; set; }
        public bool IsImage { get; set; }
        public int OrderID { get; set; }
        public string Checksum { get; set; }
    }
    public class AppFundProcessReq : AppSessionReq
    {
        public int UID { get; set; }
        public decimal Amount { get; set; }
        public bool OType { get; set; }
        public string Remark { get; set; }
        public int WalletType { get; set; }
        public int PaymentID { get; set; }
        public bool IsMarkCredit { get; set; }
    }
    public class AppUserRequest : AppSessionReq
    {
        public int RoleID { get; set; }
        public int UID { get; set; }
        public string RefferalID { get; set; }
    }
    public class AppChangePRequest : AppSessionReq
    {
        public string OldP { get; set; }
        public string NewP { get; set; }
        public string ConfirmP { get; set; }
        public bool IsPin { get; set; }
    }
    public class AppPincodeRequest : AppSessionReq
    {
        public int Pincode { get; set; }
    }
    public class AppPincodeResponse : AppResponse
    {
        public IEnumerable<PincodeDetail> Areas { get; set; }
    }
    public class AppDoubleFactorReq : AppSessionReq
    {
        public bool IsDoubleFactor { get; set; }
        public string OTP { get; set; }
        public string RefID { get; set; }
    }
    public class AppRealAPIChangeReq : AppSessionReq
    {
        public bool Is { get; set; }
    }
    public class AppUserCreate : AppSessionReq
    {
        public string Domain { get; set; }
        public UserCreate userCreate { get; set; }
        public GetEditUser editUser { get; set; }
        public int ReferralID { get; set; }

    }
    public class WebUserCreate : AppRequest
    {
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
        public string ReferralNo { get; set; }
    }
    public class HeaderInfo
    {
        public string AppID { get; set; }
        public string Version { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int SessionID { get; set; }
        public string Session { get; set; }
        public string Domain { get; set; }
    }
    public class AppUserKYCUpload : AppSessionReq
    {
        public int UID { get; set; }
        public int DocTypeID { get; set; }
    }
    public class AppKYCUpdateReq : AppSessionReq
    {
        public int OutletID { get; set; }
        public int KYCStatus { get; set; }
    }
    #endregion
    #region PlansRelated
    public class AppSimplePlanReq : AppRequest
    {
        public string AccountNo { get; set; }
        public int OID { get; set; }
        public int CircleID { get; set; }
        public string PackageID { get; set; }
        public string Language { get; set; }
    }
    public class AppROfferResp : AppResponse
    {
        public SubMplanRofferResp Data { get; set; }
        public PlanApi DataPA { get; set; }
        public CyrusPlanAPI DataCyrRof { get; set; }
        public MyPlanApi MyPlanData { get; set; }
    }
    public class AppSimplePlanResp : AppResponse
    {
        public SubMplanSimplePlanResp Data { get; set; }
        public RoundpaySimplePlanResp DataRP { get; set; }
        public PlanApiViewPlan DataPA { get; set; }
        public MyPlanApiSimplePlan MyPlanData { get; set; }
        public HLRResponseStatus HLRResponce { get; set; }
    }
    public class AppDTHSimplePlanResp : AppResponse
    {
        public SubMplanDTHSimplePlanResp Data { get; set; }
        public RoundpaySubMplanDTHSimplePlanResp DataRP { get; set; }
        public RPDTHPlansSimpleOfPackages DataRPDTHWithPackage { get; set; }
        public RPGetDTHChannelList DataRPDTHChannelList { get; set; }
        public MplanDTHHeavyRefresh DataHREF { get; set; }
        public DTHPlan DataPA { get; set; }
        public MyPlanApiDthplan MyPlanData { get; set; }
    }
    public class AppMplanDTHHeavyRefresh : AppResponse
    {
        public MplanDTHHeavyRefresh Data { get; set; }
        public RoundpayDTHHeavyRefresh DataRP { get; set; }
    }
    public class AppDTHCustInfo : AppResponse
    {
        public SubMplanDTHCustomerInfo Data { get; set; }
        public DTHCustomerInfo DataPA { get; set; }
        public MyPlanDTHCustomerInfo MyPlanData { get; set; }
    }
    #endregion
    #region DMTRelated
    public class SenderDetailReq : AppSessionReq
    {
        public _SenderRequest senderRequest { get; set; }
        public string SID { get; set; }
        public string BeneMobile { get; set; }
        public string AccountNo { get; set; }
        public string BeneID { get; set; }
        public string OTP { get; set; }
    }
    public class AddBeneRequest : AppSessionReq
    {
        public _SenderRequest senderRequest { get; set; }
        public BeneDetail beneDetail { get; set; }
        public string OTP { get; set; }
        public string SID { get; set; }
        public string TransMode { get; set; }//NEFT,IMPS
    }
    public class AppGetSenderResp : AppResponse
    {
        public bool IsSenderNotExists { get; set; }
        public bool IsEKYCAvailable { get; set; }
        public bool IsOTPGenerated { get; set; }
        public decimal RemainingLimit { get; set; }
        public decimal AvailbleLimit { get; set; }
        public string SenderName { get; set; }
        public string SenderBalance { get; set; }
    }
    public class AppBeneficiaryResp : AppResponse
    {
        public IEnumerable<AddBeni> Benis { get; set; }
        public IEnumerable<MBeneDetail> Beneficiaries { get; set; }
    }
    public class AppSendMoneyReq : AppSessionReq
    {
        public ReqSendMoney reqSendMoney { get; set; }
    }
    public class AppAmountRequest : AppSessionReq
    {
        public int Amount { get; set; }
    }
    public class AppVerificationChargeResp : AppResponse
    {
        public decimal ChargedAmount { get; set; }
    }
    public class AppDMTRecieptReq : AppSessionReq
    {
        public string GroupID { get; set; }
    }
    public class AppDMTRecieptResp : AppResponse
    {
        public TransactionDetail transactionDetail { get; set; }
    }
    public class AppValidatedUserResp : AppResponse
    {
        public string OutletName { get; set; }
        public string UName { get; set; }

    }
    public class _DMRTransactionResponse : DMRTransactionResponse
    {
        public bool IsVersionValid { get; set; }
        public bool IsAppValid { get; set; }
        public bool IsPasswordExpired { get; set; }
    }
    #endregion

    public class Authentication
    {
        public int STATUS { get; set; }
        public string Message { get; set; }
        public List<Service> Services { get; set; }
        public class Service
        {
            public int ID { get; set; }
            public string IsActive { get; set; }
            public string IsVisible { get; set; }
        }
    }

    public class AppHLRLookupReq : AppSessionReq
    {
        public string Mobile { get; set; }
    }
    public class AppHLRLookupResponse : AppResponse
    {
        public int OID { get; set; }
        public int CircleID { get; set; }
    }
    public class AppTransactionModeResponse : AppResponse
    {
        public IEnumerable<TransactionMode> TransactionModes { get; set; }
    }

    public class AppMoveToWalletReq : AppSessionReq
    {
        public int ActionType { get; set; }
        public int MTWID { get; set; }
        public decimal Amount { get; set; }
        public string TransMode { get; set; }
    }
    public class QRCodeRequest : AppSessionReq
    {
        public int Amount { get; set; }
        public string ShortURL { get; set; }
        public string spkey { get; set; }
    }
    public class AppW2RReq : AppSessionReq
    {
        public int TID { get; set; }
        public string RPID { get; set; }
        public string RightAccount { get; set; }
    }

    public class AppRefundRequestResponse : AppResponse
    {
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
        public int RefundStatus { get; set; }
        public string RefundRemark { get; set; }
        public decimal Balance { get; set; }
        public string DisputeURL { get; set; }
    }

    public class AppSetting : AppResponse
    {
        public string PlanType { get; set; }
    }

    public class BannerRequest : AppRequest
    {
        public string Domain { get; set; }
        public int OpType { get; set; }
    }

    public class BannerResponse : AppResponse
    {
        public IEnumerable<BannerImage> BannerUrl { get; set; }

    }
    public class CommonWeRequest
    {
        public int ID { get; set; }
        public string StringID { get; set; }
    }
    public class UserSubscriptionApp : AppRequest
    {
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string Message { get; set; }
        public string RequestPage { get; set; }
        public string MobileNo { get; set; }
    }
    public class _AvailablePackageForApp
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public decimal PackageCost { get; set; }
        public decimal Commission { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceMaster> Services { get; set; }
    }

    public class AvailablePackageResponse : AppResponse
    {
        public List<_AvailablePackageForApp> PDetail { get; set; }
    }

    public class UpgradePackageReq : AppSessionReq
    {
        public int AvailablePackageId { get; set; }
        public int UID { get; set; }
    }
    public class SlabRangDetailReq : AppSessionReq
    {
    }
    public class SlabRangDetailRes : AppResponse
    {
        public List<SlabRangeDetail> SlabRangeDetail { get; set; }
    }
    public class DTHPackageRequest : AppSessionReq
    {
    }
    public class DTHPackageResponse : AppResponse
    {
        public IEnumerable<DTHPackage> DTHPackage { get; set; }
    }
    public class DTHChannelRequest : AppSessionReq
    {
        public int PID { get; set; }
    }

    public class DTHChannelResponse : AppResponse
    {
        public List<DTHChannel> DTHChannels { get; set; }
    }



    #region Employee
    public class PostCreateMeetingRequest : AppSessionReq
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public int PurposeId { get; set; }
        public string Purpose { get; set; }
        public decimal Consumption { get; set; }
        public bool Isusingotherbrands { get; set; }
        public decimal Otherbrandconsumption { get; set; }
        public int ReasonId { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }
        public int AttandanceId { get; set; }
        public string MobileNo { get; set; }
        public string Latitute { get; set; }
        public string Longitute { get; set; }
        public decimal RechargeConsumption { get; set; }
        public decimal BillPaymentConsumption { get; set; }
        public decimal MoneyTransferConsumption { get; set; }
        public decimal AEPSConsumption { get; set; }
        public decimal MiniATMConsumption { get; set; }
        public decimal InsuranceConsumption { get; set; }
        public decimal HotelConsumption { get; set; }
        public decimal PanConsumption { get; set; }
        public decimal VehicleConsumption { get; set; }
        public bool IsImage { get; set; }
    }
    public class ComparisionResponse : AppResponse
    {
        public IEnumerable<ComparisionChart> data { get; set; }
    }

    public class DownlineUserResponse : AppResponse
    {
        public IEnumerable<EmpDownlineUser> data { get; set; }
    }

    public class LastDayVsTodayResponse : AppResponse
    {
        public IEnumerable<LastDayVsTodayData> data { get; set; }
    }

    public class TargetSegmentResponse : AppResponse
    {
        public IEnumerable<TargetSegment> data { get; set; }
    }

    public class UserCommitmentResponse : AppResponse
    {
        public IEnumerable<UserCommitment> data { get; set; }
    }
    public class UserCommitmentChartResponse : AppResponse
    {
        public int TotalCommitment { get; set; }
        public int TotalAchieved { get; set; }
    }
    public class EmployeeFilterRequest : AppSessionReq
    {
        public int CriteriaID { get; set; }
        public string CriteriaText { get; set; }
        public int EmployeeRole { get; set; }
        public bool SortById { get; set; }
        public bool IsDesc { get; set; }
        public int UID { get; set; }
        public int TopRows { get; set; }
    }

    public class EmployeeListResponse : AppResponse
    {
        public IEnumerable<EList> data { get; set; }
    }

    public class EmployeeUserFilterRequest : AppSessionReq
    {
        public int CriteriaID { get; set; }
        public string CriteriaText { get; set; }
        public bool SortById { get; set; }
        public bool IsDesc { get; set; }
        public int UID { get; set; }
        public int TopRows { get; set; }
        public int RoleID { get; set; }
    }

    public class EmployeeUserResponse : AppResponse
    {
        public IEnumerable<EmpUserList> data { get; set; }
    }

    public class PSTRequest : AppSessionReq
    {
        public string RequestedDate { get; set; }
        public int RoleID { get; set; }
    }

    public class PSTResponse : AppResponse
    {
        public IEnumerable<PSTReportEmp> data { get; set; }
    }

    public class TertiaryRequest : AppSessionReq
    {
        public string RequestedDate { get; set; }
        public int EmpID { get; set; }
        public int RoleID { get; set; }
    }

    public class TertiaryResponse : AppResponse
    {
        public IEnumerable<TertiaryReport> data { get; set; }
    }

    public class UserCommitmentRequest : AppSessionReq
    {
        public int EmpID { get; set; }
        public int UID { get; set; }
        public int Commitment { get; set; }
        public string Longitute { get; set; }
        public string Latitude { get; set; }
    }
    public class EmpTargetRequest : AppSessionReq
    {
        public string RequestedDate { get; set; }
        public int EmpID { get; set; }
        public int RoleID { get; set; }
    }

    public class EmpTargetResponse : AppResponse
    {
        public IEnumerable<EmployeeTargetReport> data { get; set; }
    }

    public class GetCreateMeetingResponse : AppResponse
    {
        public ReasonAndPurpuse data { get; set; }
    }



    public class GetUserByMobileRequest : AppSessionReq
    {
        public string Mobile { get; set; }
    }

    public class DataStrResponse : AppResponse
    {
        public string Data { get; set; }
    }

    public class CommonFilterRequest : AppSessionReq
    {
        public int Top { get; set; }
        public int Criteria { get; set; }
        public string DtFrom { get; set; }
        public string DtTill { get; set; }
        public string CValue { get; set; }
        public int SearchId { get; set; }
    }
    public class TodayTransactorModelResp : AppResponse
    {
        public IEnumerable<TodayTransactorsModal> Data { get; set; }
    }

    public class TodayOutletListResp : AppResponse
    {
        public IEnumerable<EmpUserList> Data { get; set; }
    }
    public class DailyClosingRequest : AppSessionReq
    {
        public decimal Travel { get; set; }
        public decimal Expense { get; set; }
    }

    public class GetAreabyPincodeRequest : AppSessionReq
    {
        public int Pincode { get; set; }
        public bool IsB2C { get; set; }
    }
    public class GetUserByMobileResponse : AppResponse
    {
        public Meetingdetails Data { get; set; }
    }
    public class GetAreabyPincodeResponse : AppResponse
    {
        public IEnumerable<PincodeDetail> Data { get; set; }
    }
    public class GetMeetingDetailResponse : AppResponse
    {
        public List<Meetingdetails> Data { get; set; }
    }
    public class PostDailyClosingResponse : AppResponse
    {
        public IResponseStatus Data { get; set; }
    }
    public class GetMeetingReportResponse : AppResponse
    {
        public List<MeetingReportModel> Data { get; set; }
    }
    public class GetMeetingSubReportResponse : AppResponse
    {
        public List<MeetingAddOnReportModel> Data { get; set; }
    }

    public class GetMapPointsResponse : AppResponse
    {
        public List<MapPointsModel> Data { get; set; }
    }
    #endregion

    public class AfItemResponse : AppResponse
    {
        public IEnumerable<AfVendorWithCategories> data { get; set; }
    }
    public class TodayLivePSTResponse : AppResponse
    {
        public IEnumerable<TodayLivePST> data { get; set; }
    }

    public class PSTDataListResp : AppResponse
    {
        public IEnumerable<PSTDataList> Data { get; set; }
    }
    #region AppLeadServiceRealated
    public class LeadAppServiceReq : AppSessionReq
    {
        public LeadService leadService { get; set; }
        public LeadServiceRequest leadServiceRequest { get; set; }
    }
    public class LeadServiceTypeModel : AppResponse
    {
        public List<LoanTypes> loanTypes { get; set; }
        public List<CustomerTypes> customerTypes { get; set; }
        public List<InsuranceTypes> insuranceTypes { get; set; }
        //public List<BankMaster> CreditCardBanks { get; set; }
    }
    public class LeadServiceModel : AppResponse
    {
        public LeadServiceRequest leadService { get; set; }
    }
    public class AppCustomerTypes : AppResponse
    {
        public int ID { get; set; }
        public string CustomerType { get; set; }
    }
    public class AppInsuranceTypes : AppResponse
    {
        public int ID { get; set; }
        public string InsuranceType { get; set; }
    }
    #endregion
    public class PlaceOrderResp : AppResponse
    {
        public string OrderId { get; set; }
    }
    public class ProfilePic : AppResponse
    {
        public int LoginTypeID { get; set; }
        public IFormFile file { get; set; }
    }
    #region MNPKendra
    public class MNPStsResp : AppResponse
    {
        public int OID { get; set; }
        public string OpName { get; set; }
        public int MNPStatus { get; set; }
        public string MNPRemark { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppLink { get; set; }
        public string MNPMobile { get; set; }

    }
    public class MNPRegistration : AppSessionReq
    {
        public string MNPMobile { get; set; }
    }
    public class MNPClaimReq : AppSessionReq
    {
        public string MNPMobile { get; set; }
        public string ReferenceID { get; set; }

    }
    public class MNPClaimDataReq : AppSessionReq
    {
        public int TopRows { get; set; }
        public string ToDate { get; set; }
        public string FromDate { get; set; }
        public bool IsRecent { get; set; }
    }
    public class MNPClaimData : AppResponse
    {
        public List<MNPClaims> MNPClaimsList { get; set; }
    }
    public class MNPClaims
    {
        public string OpName { get; set; }
        public string Status { get; set; }
        public string MNPMobile { get; set; }
        public string ReferenceID { get; set; }
        public decimal Amount { get; set; }
        public string ApprovedDate { get; set; }
    }
    #endregion
    #region WalletToWallet
    public class WtWUserDetailReq : AppSessionReq
    {
        public string MobileNo { get; set; }
    }
    public class WtWUserDetailResp : AppResponse
    {
        public WTWUserInfo UDetailsWTW { get; set; }
    }
    public class WTWFTReq : AppSessionReq
    {
        public int UID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public int WalletID { get; set; }
        public string PIN { get; set; }
    }
    #endregion


    #region FOSAccountStatement
    public class AppASLedgerReq : AppReportCommon
    {
        public int UType { get; set; }
        public int AreaID { get; set; }
        public int UID { get; set; }

    }
    public class AppFosCollectionReq : AppSessionReq
    {
        public int UID { get; set; }
        public string CollectionMode { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public int BankName { get; set; }
        public string UTR { get; set; }
    }
    public class ASColBanksResp : AppResponse
    {
        public List<ASBanks> Banks { get; set; }
    }
    public class AppFOSUserReq : AppSessionReq
    {

        public int LoginID { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
        public string Criteria { get; set; }
        public int TopRows { get; set; }
        public int RoleID { get; set; }

    }
    public class AppAMResp : AppResponse
    {
        public List<ASAreaMaster> AreaMaster { get; set; }
    }
    public class AppASColBanks : AppResponse
    {
        public List<ASBanks> Banks { get; set; }
    }
    #endregion
    public class DTHLead : AppSessionReq
    {
        public int LoginID { get; set; }
        public string Pincode { get; set; }
        public string Address { get; set; }
    }

    public class RDaddyIntARegReq
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string OutletName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public string PAN { get; set; }
        public string Aadhar { get; set; }

    }

    #region BulkQRGeneration
    public class AppMapQRToUser : AppSessionReq
    {
        public string QRIntent { get; set; }
    }
    #endregion
    #region RNPPlans
    public class AppMNPSimpResp : AppResponse
    {
        public object Data { get; set; }
        public List<RNPRofferData> RofferData { get; set; }
        public RNPDTHCustInfo DTHCIData { get; set; }
        public RNPDTHHeavyRefresh DTHHRData { get; set; }
    }
    #endregion

    public class WebWTWUserInfo: OperatorResponse
    {
        public WTWUserInfo wtwuserinfo { get; set; }
      
    }

}