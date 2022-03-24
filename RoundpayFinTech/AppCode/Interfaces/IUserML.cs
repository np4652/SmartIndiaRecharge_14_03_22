using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IUserML
    {

        IResponseStatus SwitchIMPStoNEFT(bool Status);
        Task<IResponseStatus> EnableGoogleAuthenticator(bool IsGoogle2FAEnable, string AccountSecretKey = "", bool Action = false, int userId = 0);
        Task<IResponseStatus> ResetGoogleAuthenticator(int userId);
        IResponseStatus DebitRquesttStatus(int TableID, int status, bool MarkAsDebit,string Remark);
        IEnumerable<DebitFundrequest> DebitFundRequest(DebitFundrequest data);

        IResponseStatus IDLimit(GetAddService req);
        ResponseStatus UpdateUAA(UpdateUserReq updateUserReq);
        AAUserData GetUserAggrement(AAUserReq aAUserReq);
        List<MasterCompanyType> GetCompanyTypeMaster(CommonReq commonReq);
        #region FOSAS
        UserList AppGetList(CommonFilter _filter);
        #endregion
        ResponseStatus WTWFT(CommonReq commonReq);
        WTWUserInfo GetUserByMobile(CommonReq commonReq);
        ResponseStatus ChangeAPISwSts(int Id);
        ResponseStatus BulkDebitCredit(CommonReq commonReq);
        IResponseStatus ChangeFOSColSts(int UserID);
        IResponseStatus BonafideAccountSetting(int AccountID, bool IsDelete);
        IEnumerable<BonafideAccount> GetBonafideAccount(CommonReq _req);
        ResponseStatus UploadInvoice(IFormFile file, string InvoiceMonth, bool IsRCM);
        AutoBillingModel GetAutoBillingDetail(int id);
        Task<List<AlertReplacementModel>> GetAutoBillingProcess(int userId);
        IResponseStatus UpdateAutoBilling(AutoBillingModel req);
        IResponseStatus RegisterasVendor(int UserID);
        Task<ResponseStatus> IsVendor(int UserID = 0);
        IResponseStatus UpdateFOS(GetEditUser _req);
        IResponseStatus DeleteWebNotification(string Ids, int Action = 2);
        IEnumerable<CustomerCareDetails> GetCustomercare();
        Task<LeadSummary> GetLeadSummary(int CustomerID);
        DTHSubscriptionReport GetBookingStatus(int ID, string TransactionID);
        ResponseStatus EMailVerify(int UserID);
        IResponseStatus SaveHour(CommonReq _req);
        IEnumerable<PincodeDetail> GetPincodeArea(CommonReq req);
        AlertReplacementModel GetUserDeatilForAlert(int UserID);
        bool SendVerifyEmailLink(LoginResponse _applr = null);
        //ResponseStatus UpdateUserDenominationStatus(int UserID);
        IResponseStatus ChangeFlatCommStatus(int UserID);
        List<MasterRole> GetMasterRole();
        ResponseStatus UpdateMasterRoel(int Id, int RegCharge);
        Task<ResponseStatus> CallBankTransfer(BankServiceReq bankServiceReq);
        IResponseStatus RemoveUserSubscription(CommonReq _req);
        UserDetail GetUserDetailByID(string MobileNo);
        UserRegModel GetReffDeatil(string MobileNo);
        bool IsCustomerCareAuthorised(string OperationCode);
        ResponseStatus CallCreateUser(UserCreate _req);
        UserRoleSlab GetRoleSlab();
        UserList GetList(CommonFilter _filter);
        UserList GetListChild(int ReffID, bool IsUp);
        UserList GetFOSList(CommonFilter _filter);
        IResponseStatus ChangeOTPStatus(int UserID, int type, bool Is);
        GetEditUser GetEditUser(int UserID);
        IResponseStatus UpdateUser(GetEditUser req);
        IResponseStatus UpdateProfile(GetEditUser _req);
        bool IsEndUser();
        UserInfo GetUser(string MobileNo, int UT = 1);
        GetChangeSlab fn_GetChangeSlab(int UserID);
        ResponseStatus UpdateChangeSlab(int UserID, int SlabID, string pinPassword);
        UserBalnace GetUserBalnace(int UID);
        UserBalnace GetUserBalnace(int UserID, int LoginTypeID);
        PincodeDetail GetPinCodeDetail(string Pincode);
        AlertReplacementModel ChangePassword(ChangePassword ChPass);
        AlertReplacementModel ChangePin(ChangePassword ChPass);
        List<ResponseStatus> GetUpperRole(int ID);
        Dashboard DashBoard();
        MiddleLayerUser MiddleDashBoard();
        Task<List<BulkExcel>> BulkActionFixedRoles(CommonReq req);
        //ResponseStatus BulkDebitCredit(CommonReq commonReq);
        UserRegModel GetReffDeatilFromBulk(string MobileNo);
        List<RoleMaster> GetChildRole();
        UserBalnace ShowTotalChildBalance(int UserID);
        List<Notification> GetNotifications();
        IResponseStatus RemoveNotification(int ID);
        List<RoleMaster> GetChildRole(int RoleID);
        bool SendUserOTPForCommonThings(string OTP, int WID, string MobileNo, string EmailID);
        IResponseStatus GetOTPFromURL(string encdata);
        IResponseStatus ChangeDoubleFactorFromApp(ProcToggleStatusRequest _req);
        List<UserReportBulk> BulkAction(CommonReq req);
        IEnumerable<TransactionMode> GetTransactionMode();
        TransactionMode GetTransactionMode(CommonReq commonReq);
        IResponseStatus UpdateTransactionMode(CommonReq commonReq);
        IResponseStatus Regeneratepassword(int UserID);
        ResponseStatus CallSignup(UserCreate _req);
        IResponseStatus UpdateFlatComm(decimal req, int UserID);
        IResponseStatus PartialUpdate(UserCreate param);
        LowBalanceSetting GetSetting(int UserID = 0);
        bool IsEMailVerified(int UserID = 0);
        IResponseStatus SaveLowBalanceSetting(LowBalanceSetting setting);
        IEnumerable<UserCallMeModel> GetCallMeRequests(int t);
        ResponseStatus UpdateCallMeHistory(UserCallMeModel data);
        int CallMeRequestCount();
        UserRegModel GetRoleSignUp();
        IResponseStatus ChangeVirtualStatus(int UserID);
        IEnumerable<DMRModel> GetDMRModelList();
        IResponseStatus UserBankRequest(GetEditUser _req);
        GetinTouctListModel GetUserSubcription(CommonReq _req);
        IResponseStatus AssignuserSubscription(CommonReq _req);
        GetinTouctListModel ProcGetUserSubscriptionCusCare(CommonReq _req);
        IResponseStatus UpdateuserSubscriptionStatusCuscare(CommonReq _req);
        IResponseStatus UpdateuserSubscriptionRemarksCuscare(CommonReq _req);
        IEnumerable<UserInfo> GetAllUplines(int UserID);
        IResponseStatus ChangeSlabStatus(int ID);
        List<MoveToWalletMapping> GetMoveToWalletsMap();
        SettlementSetting GetSettlementSetting(int uid);
        ResponseStatus GetSettlementSetting(UserwiseSettSetting userwiseSettSetting);
        SettlementSetting GetSettlementSettingSeller();
        IResponseStatus UpdateMNPClaiimStatus(int Status, int ID, string Remark, decimal Amount, string FRCDate, string FRCDemoNo, string FRCType, string FRCDoneDate);
        MNPDetailsResp GetMNPClaimByID(MNPUser req);
        IResponseStatus UpdateMNPStatus(int Status, int ID, string UserName, string Password, string Remark, string Demo);
        MNPDetailsResp GetMNPUserByID(MNPUser req);
        MNPUserResp GetMNPUser(int UserID, string s = "");
        MNPUserResp GetMNPCliam(int UserID, string s = "");
        IResponseStatus ToggleCandebitDownLineStatus(int UserID, bool Is);
        IResponseStatus ToggleCandebitStatus(int UserID, bool Is);
        IResponseStatus _WallettoWallet(CommonReq commonreq);
        IEnumerable<UserBillFetchReport> GetUserBillFetch(UserBillFetchReport Data);


        Task<IEnumerable<Team>> GetTeam(int UserId);
        Task<IEnumerable<Level>> GetLevel(int UserId);

    }
}
