using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IMoneyTransferML
    {
        MSenderCreateResp CreateSender(MTSenderDetail request);
        MSenderLoginResponse GetSender(MTCommonRequest request);
        MSenderCreateResp SenderKYC(MTSenderDetail request);
        MSenderCreateResp SenderEKYC(MTSenderDetail request);
        MSenderCreateResp SenderResendOTP(MTOTPRequest request);
        MSenderLoginResponse VerifySender(MTOTPRequest request);
        MSenderLoginResponse CreateBeneficiary(MTBeneficiaryAddRequest request);
        MBeneficiaryResp GetBeneficiary(MTCommonRequest request);
        MSenderCreateResp GenerateOTP(MTBeneficiaryAddRequest request);
        MSenderLoginResponse ValidateBeneficiary(MBeneVerifyRequest request);
        MSenderLoginResponse RemoveBeneficiary(MBeneVerifyRequest request);
        MSenderLoginResponse ValidateRemoveBeneficiary(MBeneVerifyRequest request);
        MDMTResponse VerifyAccount(MBeneVerifyRequest request);
        MDMTResponse AccountTransfer(MBeneVerifyRequest request);
        ChargeAmount GetCharge(MTGetChargeRequest request);
        MDMTResponse DoUPIPaymentService(MBeneVerifyRequest request);
    }
}
