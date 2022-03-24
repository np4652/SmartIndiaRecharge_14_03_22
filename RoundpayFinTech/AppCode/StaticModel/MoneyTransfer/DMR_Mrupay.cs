using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.StaticModel.MoneyTransfer
{
    public class DMR_Mrupay
    {
    }
    public class DMR_Common
    {
        public string status { get; set; }
        public string ErrorCode { get; set; }
        public string msg { get; set; }
    }
    public class SenderLimit:DMR_Common
    {
        public string Remaining_Limit { get; set; }
    }
    public class MRuyCreateSender
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobileno { get; set; }
        public string pincode { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
    }
    public class MRuyCreateSenderRes: DMR_Common
    {
        public string IsOTP { get; set; }
        public string OTP_Reference { get; set; }
        public string LiveId { get; set; }
    }
    public class AccountDetail
    {
        public string BeneficiaryCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankName { get; set; }
    }
    public class MryAddGetBeneficiary
    {
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC { get; set; }
    }
    public class MryGetBeneficiary
    {
        public string status { get; set; }
        public string ErrorCode { get; set; }
        public string msg { get; set; }
        public string SenderMobileNo { get; set; }
        public List<AccountDetail> Account_Detail { get; set; }
    }
    public class MrySendMoney
    {
        public string RequestId { get; set; }
        public string AccountNumber { get; set; }
        public string Amount { get; set; }
        public string BeneficiaryId { get; set; }
        public string Channel { get; set; }
    }
    public class MryRespSendMoney : DMR_Common
    {
        public string LiveId { get; set; }
        public string TransactionId { get; set; }
        public string RequestId { get; set; }
        public string AccountHolderName { get; set; }
    }
    public class MryVerification
    {
        public string AccountNumber { get; set; }
        public string IFSC_Or_BankCode { get; set; }
        public string RequestId { get; set; }
    }
    public class MryVerificationResp
    {
        public string status { get; set; }
        public string ErrorCode { get; set; }
        public string msg { get; set; }
        public string AccountHolderName { get; set; }
        public string IFSC { get; set; }
    }
}
