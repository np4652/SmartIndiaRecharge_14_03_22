using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class PrimePayML : IMoneyTransferAPIML
    {
       

        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
