using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IVerificationAPI
    {
        VerificationServiceRes VerifyAccount(VericationServiceReq serviceReq);
        VerificationServiceRes VerifyUPIID(VericationServiceReq serviceReq);
    }
    public interface IVerificationML
    {
        VerificationOutput AccountVerification(VerificationInput verificationInput);
        VerificationOutput UPIVerification(VerificationInput verificationInput);
    }
}
