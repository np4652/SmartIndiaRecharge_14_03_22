using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ITransactionService
    {
        Task<IActionResult> Balance(APIRequest req);
        Task<IActionResult> StatusCheck(StatusAPIRequest req);
        Task<IActionResult> TransactionAPI(RechargeAPIRequest req);
    }
    public interface IDMTTransactionService
    {
        Task<IActionResult> GetSender([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> CreateSender([FromBody] CreateSednerReq dMTAPIRequest);
        Task<IActionResult> VerifySender([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> AddBeneficiary([FromBody] CreateBeneReq dMTAPIRequest);
        Task<IActionResult> DeleteBeneficiary([FromBody] DeleteBeneReq dMTAPIRequest);
        Task<IActionResult> GetBeneficiary([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> GenerateBenficiaryOTP([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> ValidateBeneficiary([FromBody] CreateBeneReq dMTAPIRequest);
        Task<IActionResult> VerifyAccount([FromBody] DMTTransactionReq dMTAPIRequest);
        Task<IActionResult> SendMoney([FromBody] DMTTransactionReq dMTAPIRequest);
        IActionResult SplitAmountMethod(int RAmount, int m, int x);
    }

    public interface IDMTTransactionServiceP
    {
        Task<IActionResult> GetSender([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> CreateSender([FromBody] CreateSednerReq dMTAPIRequest);
        Task<IActionResult> SenderKYC(int UserID, string Token, int OutletID, string SenderMobile, string ReferenceID, string SPKey, string NameOnKYC, string AadharNo, string PANNo, IFormFile AadharFront, IFormFile AadharBack, IFormFile SenderPhoto, IFormFile PAN);
        Task<IActionResult> VerifySender([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> SenderResendOTP([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> AddBeneficiary([FromBody] CreateBeneReq dMTAPIRequest);
        Task<IActionResult> DeleteBeneficiary([FromBody] DeleteBeneReq dMTAPIRequest);
        Task<IActionResult> ValidateRemoveBeneficiaryOTP([FromBody] DMTTransactionReq dMTAPIRequest);
        Task<IActionResult> GetBeneficiary([FromBody] DMTAPIRequest dMTAPIRequest);
        Task<IActionResult> GenerateBenficiaryOTP([FromBody] DeleteBeneReq dMTAPIRequest);
        Task<IActionResult> ValidateBeneficiary([FromBody] DMTTransactionReq dMTAPIRequest);
        Task<IActionResult> VerifyAccount([FromBody] DMTTransactionReq dMTAPIRequest);
        Task<IActionResult> AccountTransfer([FromBody] DMTTransactionReq dMTAPIRequest);
    }
    public interface IAEPSTransactionService
    {
        Task<IActionResult> GenerateAEPSURL([FromBody] PartnerAPIRequest aPIRequest);
    }
    public interface IAPIOutletService
    {
        Task<IActionResult> RegisterOutlet([FromBody] APIRequestOutlet aPIRequest);
        Task<IActionResult> UpdateOutlet([FromBody] APIRequestOutlet aPIRequest);
        Task<IActionResult> OutletStatuscheck([FromBody] APIRequestOutlet aPIRequest);
        Task<IActionResult> OutletServiceStatus([FromBody] APIRequestOutlet aPIRequest);
        IActionResult GetResource([FromBody] APIRequestOutlet aPIRequest);
    }
    public interface IAPIPayoutDMTService
    {
        Task<IActionResult> DMTPayout([FromBody] PayoutTransactionRequest payoutRequest);
    }
}
