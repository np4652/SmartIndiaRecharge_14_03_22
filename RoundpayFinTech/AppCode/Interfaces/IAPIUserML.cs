using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAPIUserMiddleLayer
    {
        Task<BillerAPIResponse> GetRPBillerByType(APIBillerRequest req);
        Task<BillerAPIResponse> GetRPBillerByID(APIBillerRequest req);
        IResponseStatus AddMAtm(AddmAtmModel model);
        bool TestAPI();
        Task<StatusMsg> GetBalance(APIRequest req);
        Task<TransactionResponse> APIRecharge(RechargeAPIRequest req);
        Task<TransactionResponse> APIPANService(RechargeAPIRequest req);
        Task<TransactionResponse> FetchBill(RechargeAPIRequest req);
        Task SaveAPILog(APIReqResp aPIReqResp);
        bool BolckSameRequest(string _Method, string _Req);
        Task<TransactionResponse> GetStatusCheck(StatusAPIRequest req);
        Task<DMTCheckStatusResponse> GetDMTStatusCheck(StatusAPIRequest req);
        Task SaveDMRAPILog(APIReqResp aPIReqResp);
        Task<StatusMsg> MarkDispute(RefundAPIRequest req);
        #region DMTAPIRegion
        Task<SenderLoginResponse> SenderLogin(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> CreateSender(CreateSednerReq dMTAPIRequest);
        Task<DMTAPIResponse> VerifySender(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> CreateBeneficiary(CreateBeneReq dMTAPIRequest);
        Task<SenderCreateResponse> DeleteBeneficiary(DeleteBeneReq dMTAPIRequest);
        Task<BeneResponse> GetBeneficiary(DMTAPIRequest dMTAPIRequest);
        Task<SenderCreateResponse> GenerateBenficiaryOTP(DMTAPIRequest dMTAPIRequest);
        Task<DMTAPIResponse> ValidateBeneficiary(CreateBeneReq dMTAPIRequest);
        Task<DMTTransactionResponse> VerifyAccount(DMTTransactionReq dMTAPIRequest);
        Task<DMTTransactionResponse> SendMoney(DMTTransactionReq dMTAPIRequest);
        #endregion
        #region AEPSRegion
        Task<GenerateURLResponse> GenerateAEPSURL(PartnerAPIRequest aPIRequest);        
        #endregion
    }
    public interface IOutletAPIUserMiddleLayer {
        ServiceResponse SaveOutletOfAPIUser(APIRequestOutlet aPIRequest);
        ServiceResponse UpdateOutletOfAPIUser(APIRequestOutlet aPIRequest);
        ServiceResponse CheckOutletStatus(APIRequestOutlet aPIRequest);
        ServiceResponse CheckOutletServiceStatus(APIRequestOutlet aPIRequest);
        ServiceResponse GetResources(APIRequestOutlet aPIRequest);
    }
}
