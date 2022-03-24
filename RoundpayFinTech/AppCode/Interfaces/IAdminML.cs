using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAdminML
    {
        ResponseStatus PGDownLineStsChange(int UserID);
        ResponseStatus PGStsChange(int UserID);
        IResponseStatus UpdatCalCommCir(int UserID, bool Is);
        string ShowPasswordCustomer(int ID);
        IResponseStatus UpdateMarkRG(int UserID, bool Is);
        Task<IResponseStatus> AddWallet(FundProcessReq _req);
        IResponseStatus SendNotification(Notification req);
        string ShowPassword(int ID);
        IResponseStatus UpdateChangeRole(int UserID, int RoleID,int LoginId,int LoginTypeId);
        IResponseStatus UpdateInvoiceByAdmin(int UserID, bool Is);
        IResponseStatus SendUTROTP(int userId);
        Task<IResponseStatus> UploadUTRExcelAsync(List<UTRExcel> records);
        Task<IEnumerable<UTRExcelMaster>> GetUTRExcelAsync();
        Task<IEnumerable<UTRExcel>> GetUTRDetailExcelAsync(int FileId);
        Task<IEnumerable<UTRExcelMaster>> GetUTRStatementAsync();
        Task<IEnumerable<UtrStatementUpload>> DownloadUTRStatementAsync(int FiledID);

    }
}
