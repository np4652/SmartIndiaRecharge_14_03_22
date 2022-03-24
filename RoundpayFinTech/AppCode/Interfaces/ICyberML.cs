using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ICyberML
    {
    }
    public interface ICyberDMTMLPayTM
    {
        ResponseStatus RemitterDetailsPTM(DMTReq _req);
        ResponseStatus RemitterLimitPTM(DMTReq _req);
        ResponseStatus GetOTPForRegistrationPTM(DMTReq _req);
        ResponseStatus RemitterRegistrationPTM(CreateSen createSen, string state);
        ResponseStatus BeneficiaryRegistrationPTM(AddBeni addBeni, DMTReq _req);
        BeniRespones ListOfBeneficiaryPTM(DMTReq _req);
        DMRTransactionResponse BeneAccountValidationPTM(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res);
        DMRTransactionResponse FundTransferPTM(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res);
    }
    public interface ICyberDMTMLIPay
    {
        BeniRespones RemitterDetailsIPay(DMTReq _req);
        ResponseStatus RemitterRegistrationIPay(CreateSen createSen);
        ResponseStatus RemitterOTPVerifyIPay(DMTReq _req, string OTP);
        ResponseStatus BeneficiaryRegistration(AddBeni addBeni, DMTReq _req);
        DMRTransactionResponse BeneAccountValidationIPay(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res);
        DMRTransactionResponse FundTransferIPay(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res);
        ResponseStatus BeneficiaryDeleteIPay(DMTReq _req, string BeniID);
        ResponseStatus BeneficiaryDeleteValidateIPay(DMTReq _req, string BeniID, string otp);
    }
}
