using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.ThirdParty.Eko;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IRoundpayApiML
    {
        OutletAPIStatusUpdate CheckOnboardStatus(RoundpayApiRequestModel ObjRoundpayApiRequestModel);
        OutletAPIStatusUpdate OutRegistration(RoundpayApiRequestModel ObjRoundpayApiRequestModel);
        OutletAPIStatusUpdate ServicePlus(RoundpayApiRequestModel ObjRoundpayApiRequestModel);
        BCResponse GetBCDetail(RoundpayApiRequestModel ObjRoundpayApiRequestModel);
        Task<PSAResponse> CouponRequest(RoundpayApiRequestModel ObjRoundpayApiRequestModel);
        GenerateURLResp GenerateToken(GeneralInsuranceDBResponse generalInsuranceDBResponse);
    }
    public interface IMahagramAPIML
    {
        OutletAPIStatusUpdate APIBCRegistration(MGOnboardingReq MGReq, int APIID);
        OutletAPIStatusUpdate APIBCStatus(MGBCStatusRequest MGReq, int APIID);
        OutletAPIStatusUpdate BCInitiate(MGInitiateRequest MGReq, int APIID);
        OutletAPIStatusUpdate GetBCCode(MGBCGetCodeRequest MGReq, int APIID);
        OutletAPIStatusUpdate UTIRegistration(MGPSARequest MGReq, int APIID);
        OutletAPIStatusUpdate UTIRegistrationUpdate(MGPSARequest MGReq, int APIID);
        OutletAPIStatusUpdate UTIAgentStatuscheck(MGPSARequest MGReq, int APIID);
        Task<PSAResponse> UTICouponRequest(MGCouponRequest MGReq);
        HitRequestResponseModel UTICouponStatus(MGCouponRequest MGReq);
        MiniBankTransactionServiceResp MiniBankStatusCheck(MBStatusCheckRequest MGReq);
    }

    public interface IMTEKO
    {
        Task<EKOClassess> GetCustomerInformation(EKORequest eKORequest);
        Task<EKOClassess> CreateCustomer(EKORequest eKORequest);
        Task<EKOClassess> CreateCustomerforKYC(EKORequest eKORequest);
        Task<EKOClassess> VerifyCustomerIdentity(EKORequest eKORequest);
        Task<EKOClassess> ResendOTP(EKORequest eKORequest);
        Task<EKOClassess> GetListofRecipients(EKORequest eKORequest);
        Task<EKOClassess> GetRecipientDetails(EKORequest eKORequest);
        Task<EKOClassess> AddRecipient(EKORequest eKORequest);
        Task<EKOClassess> RemoveRecipient(EKORequest eKORequest);
        Task<EKOClassess> AccountVerification(EKORequest eKORequest, bool IsByBankCode);
        Task<EKOClassess> InitiateTransaction(EKORequest eKORequest);
        Task<EKOClassess> GetTransactionStatus(EKORequest eKORequest);
        Task<EKOClassess> Refund(EKORequest eKORequest);
        Task<EKOClassess> ResendRefundOTP(EKORequest eKORequest);
    }
}
