using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IEKYCML {
        EKYCGetDetail GetEKYCDetailOfUser(CommonReq commonReq);
        EKYCGSTINResponseModel ValidateGST(EKYCRequestModel req);
        EKYCByAadharModelOTP GenerateAadharOTP(EKYCRequestModel req);
        EKYCByAadharModel ValidateAadharOTP(EKYCRequestModel req);
        EKYCByPANModel GetPanDetail(EKYCRequestModel req);
        EKYCByBankAccountModel GetBankAccountDetail(EKYCRequestModel req);
        ResponseStatus EditEKYCStep(EKYCRequestModel req);
    }
    public interface IKYCML
    {
        
        IEnumerable<DocTypeMaster> UsersKYCDetails(int UserID);
        ResponseStatus CheckKycStatus(int UserID);
        IEnumerable<DocTypeMaster> GetDocTypeMaster(bool IsOutlet);
        IResponseStatus UpdateDocTypeMaster(DocTypeMaster docTypeMaster);
        IEnumerable<DocTypeMaster> GetDocuments(int uid);
        IResponseStatus UploadDocuments(IFormFile file, int dtype, int uid);
        Task<IEnumerable<GetEditUser>> GetKYCUsers(UserRequest req);
        IEnumerable<DocTypeMaster> GetDocumentsForApproval(int UserID, int OutletID);
        IResponseStatus ChangeKYCStatus(KYCStatusReq kYCStatusReq);
        List<KYCDoc> GetOnBoardKyc(int UserID, int OutletID);
        IEnumerable<DocTypeMaster> GetUserDocumentsList(int UserID);
    }
}
