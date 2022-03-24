using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IUserAPPML
    {
        UserList GetFOSList(CommonReq _filter);
        IResponseStatus GenerateOrderForUPI(GenerateOrderUPIRequest orderUPIRequest);
        IEnumerable<UserReportBulk> BulkActionApp(CommonReq req);
        IEnumerable<PaymentModeMaster> PaymentModesForApp(CommonReq commonReq);
        IResponseStatus FundRequestOperationApp(FundRequest fundRequest);
        IResponseStatus FundTransferApp(FundProcessReq fundProcessreq);
        IEnumerable<RoleMaster> GetUserChildRolesApp(int UserID);
        IResponseStatus ChangeUserStatusFromApp(ProcToggleStatusRequest _req);
        IResponseStatus FundRejectFromApp(FundProcessReq req);
        UserDetail GetAppUserDetailByID(UserRequset req);
        UserRegModel GetAppUserReffDeatil(UserRequset req);
        IResponseStatus CallCreateUserApp(UserCreate _req);
        IEnumerable<DocTypeMaster> GetDocumentsForApp(DocTypeMaster docTypeMaster);
        GetEditUser GetEditUserForApp(CommonReq commonReq);
        IResponseStatus UpdateUserFromApp(GetEditUser _req);
        IResponseStatus UploadDocumentsForApp(IFormFile file, int dtype, int uid, int LoginTypeID, int LoginID);
        IResponseStatus ChangeKYCStatusForApp(KYCStatusReq kYCStatusReq);
        List<Notification> GetNotificationsApp(int UserID);

        AlertReplacementModel ChangePin(ChangePassword ChPass, int LoginTypeID, int UserID);
        AlertReplacementModel ChangePassword(ChangePassword ChPass, int LoginTypeID, int UserID);
        CompanyProfileDetail GetCompanyProfileApp(int WID);
        IEnumerable<TransactionMode> GetTransactionModes(CommonReq commonReq);
        Task<IResponseStatus> MoveToWalletApp(CommonReq commonReq);
        Task<IResponseStatus> CallSignupWebApp(UserCreate _req);
        IResponseStatus CallSignupFromApp(UserCreate _req);
        IResponseStatus UserBankRequestApp(GetEditUser _req);
        IResponseStatus UpdateUserFromAppB2C(GetEditUser _req);
    }
}
