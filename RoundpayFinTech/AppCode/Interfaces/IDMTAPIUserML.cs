using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IDMTAPIUserML
    {
        Task<SenderLoginResponse> GetSender(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> CreateSender(CreateSednerReq dMTAPIRequest);
        Task<SenderCreateResponse> SenderKYC(int UserID, string Token, int OutletID, string SenderMobile, string ReferenceID, string SPKey, string NameOnKYC, string AadharNo, string PANNo, IFormFile AadharFront, IFormFile AadharBack, IFormFile SenderPhoto, IFormFile PAN);
        Task<DMTAPIResponse> VerifySender(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> SenderResendOTP(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> CreateBeneficiary(CreateBeneReq dMTAPIRequest);
        Task<APIDMTBeneficiaryResponse> GetBeneficiary(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> GenerateOTPBeneficiary(DeleteBeneReq dMTAPIRequest);
        Task<SenderCreateResponse> RemoveBeneficiary(DeleteBeneReq dMTAPIRequest);
        Task<SenderCreateResponse> ValidateRemoveBeneficiaryOTP(DMTTransactionReq dMTAPIRequest);
        Task<DMTAPIResponse> ValidateBeneficiaryOTP(DMTTransactionReq dMTAPIRequest);
        Task<DMTTransactionResponse> VerifyAccount(DMTTransactionReq dMTAPIRequest);
        Task<DMTTransactionResponse> AccountTransfer(DMTTransactionReq dMTAPIRequest);
        Task<DMTTransactionResponse> DoUPIPayment(DMTTransactionReq dMTAPIRequest);
    }
}
