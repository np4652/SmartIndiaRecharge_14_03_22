using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IMoneyTransferAPIML
    {
        MSenderCreateResp CreateSender(MTAPIRequest request);
        MSenderLoginResponse GetSender(MTAPIRequest request);
        MSenderCreateResp SenderKYC(MTAPIRequest request);
        MSenderCreateResp SenderEKYC(MTAPIRequest request);
        MSenderCreateResp SenderResendOTP(MTAPIRequest request);
        MSenderLoginResponse VerifySender(MTAPIRequest request);
        MSenderLoginResponse CreateBeneficiary(MTAPIRequest request);
        MBeneficiaryResp GetBeneficiary(MTAPIRequest request);
        MSenderCreateResp GenerateOTP(MTAPIRequest request);
        MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request);
        MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request);
        MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request);
        DMRTransactionResponse VerifyAccount(MTAPIRequest request);
        DMRTransactionResponse AccountTransfer(MTAPIRequest request);
    }
    
}
