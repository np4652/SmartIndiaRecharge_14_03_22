using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ITransactionML
    {
        Task<GenerateURLResponse> GenerateAEPSURL(PartnerAPIRequest aPIRequest);
        Task<ResponseStatus> ValidateAEPSURL(AEPSURLSessionResp aEPSURLSessionResp);        
    }
    public interface IDmtML {
        IResponseStatus CheckRepeativeTransaction(int SessionID, int UserID, decimal Amount, string AccountNo, int ID);
        ValidateAPIOutletResp GetOutlet(DMTReq dMTReq);
        Task<IResponseStatus> CheckSender(DMTReq dMTReq);
        Task<IResponseStatus> CreateSender(CreateSen _req);
        Task<IResponseStatus> SenderResendOTP(DMTReq dMTReq);
        Task<IResponseStatus> VerifySender(DMTReq dMTReq, string OTP);
        Task<IResponseStatus> CreateBeneficiary(AddBeni addBeni, DMTReq dMTReq);
        Task<IResponseStatus> GenerateOTP(DMTReq dMTReq);
        Task<IResponseStatus> ValidateBeneficiary(DMTReq dMTReq, string BeneMobile, string AccountNo, string OTP);
        Task<BeniRespones> GetBeneficiary(DMTReq dMTReq);
        Task<DMRTransactionResponse> SendMoney(DMTReq dMTReq, ReqSendMoney sendMoney);
        DMRTransactionResponse DoDMT(DMTReq dMTReq, ReqSendMoney sendMoney, ValidateAPIOutletResp _ValResp);
        Task<IResponseStatus> DeleteBeneficiary(DMTReq dMTReq, string BeniID,string OTP);
        Task<DMRTransactionResponse> Verification(DMTReq dMTReq, ReqSendMoney veri);
        
    }

    public interface IDTHSubscriptionML {
        Task<DTHConnectionServiceResponse> DoDTHSubscription(DTHConnectionServiceRequest dTHConnectionServiceRequest);
    }

}
