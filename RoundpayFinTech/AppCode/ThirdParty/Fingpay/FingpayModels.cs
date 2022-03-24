using System.Collections.Generic;
using System.Security.Permissions;

namespace RoundpayFinTech.AppCode.ThirdParty.Fingpay
{
    public class FingpayOTPResp
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
        public FingpayOTPData data { get; set; }
    }
    public class FingpayTwowayResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
        public FingpayTwowayResponseData data { get; set; }
    }
    public class FingpayTwowayResponseData
    {
        public string tefPkId { get; set; }
        public string stan{ get; set; }
        public string bankRRN { get; set; }
        public string fpRrn { get; set; }
        public string fingpayTransactionId { get; set; }
        public string merchantTranId { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string mobileNumber { get; set; }
        public string transactionTimestamp { get; set; }
    }
    public class FingpayOTPData
    {
        public int primaryKeyId { get; set; }
        public string encodeFPTxnId { get; set; }
    }
    public class FingpayReq
    {
        public string username { get; set; }
        public string password { get; set; }
        public string timestamp { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int supermerchantId { get; set; }
        public List<FingPMerchantModel> merchants { get; set; }
        public bool ShouldSerializetimestamp() => false;
    }
    public class FingPMerchantModel
    {
        public int merchantId { get; set; }
        public string merchantLoginId { get; set; }
        public string merchantLoginPin { get; set; }
        public string merchantName { get; set; }
        public FingPMerchantAddress merchantAddress { get; set; }
        public string merchantPhoneNumber { get; set; }
        public string companyLegalName { get; set; }
        public string companyMarketingName { get; set; }
        public string merchantBranch { get; set; }
        public FingPKYC kyc { get; set; }
        public FingPSettlement settlement { get; set; }
        public string emailId { get; set; }
        public string shopAndPanImage { get; set; }
        public string cancellationCheckImages { get; set; }
        public string ekycDocuments { get; set; }
        public string merchantPinCode { get; set; }
        public string tan { get; set; }
        public string merchantCityName { get; set; }
        public string merchantDistrictName { get; set; }
    }
    public class FingPMerchantAddress
    {
        public string merchantAddress { get; set; }
        public string merchantState { get; set; } // should get the merchant state from the api mentioned above in step 1
    }
    public class FingPKYC
    {
        public string userPan { get; set; }
        public string aadhaarNumber { get; set; }
        public string gstInNumber { get; set; }
        public string companyOrShopPan { get; set; }
    }
    public class FingPSettlement
    {
        public string companyBankAccountNumber { get; set; }
        public string bankIfscCode { get; set; }
        public string companyBankName { get; set; }
        public string bankBranchName { get; set; }
        public string bankAccountName { get; set; }
    }

    public class FingpayResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long statusCode { get; set; }
        public FingpayRespData data { get; set; }
    }
    public class FingpayRespData
    {
        public string terminalId { get; set; }
        public string requestTransactionTime { get; set; }
        public double transactionAmount { get; set; }
        public double balanceAmount { get; set; }
        public string transactionStatus { get; set; }
        public string bankRRN { get; set; }
        public string transactionType { get; set; }
        public string fpTransactionId { get; set; }
        public string merchantTxnId { get; set; }
        public string responseCode { get; set; }
        public List<FingPMerchantModel> merchants { get; set; }
    }

    public class FPMiniBankStatusCheckRequest
    {
        public string merchantTranId { get; set; }
        public string hash { get; set; }
        public string merchantLoginId { get; set; }
        public string merchantPassword { get; set; }
        public int superMerchantId { get; set; }
        public string superMerchantPassword { get; set; }

    }
    public class FPMiniBankStatusCheckResponse
    {
        public bool apiStatus { get; set; }
        public bool status { get; set; }
        public string apiStatusMessage { get; set; }
        public string message { get; set; }
        public List<FPMiniBankStatusCheckData> data { get; set; }
        public long apiStatusCode { get; set; }
        public long statusCode { get; set; }

    }
    public class FPCallbackUpdateStatus {
        public string ipaddress { get; set; }
        public decimal amount { get; set; }
        public string transactionStatus { get; set; }
        public string merchantRefNo { get; set; }
        public string fpTransactionId { get; set; }
        public string aadhaarNumber { get; set; }
        public string typeOfTransaction { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string mobile { get; set; }
        public string errorMessage { get; set; }
        public string bankRRN { get; set; }
        public string merchantName { get; set; }
        public string terminalID { get; set; }
        public string bankName { get; set; }
        public string requestedTimestamp { get; set; }
        public string merchantID { get; set; }
        public string deviceIMEI { get; set; }
        public string cardNumber { get; set; }
        public string cardType { get; set; }
        public decimal balance { get; set; }
        public string mposSerialNumber { get; set; }
    }
    public class FPMiniBankStatusCheckData
    {
        public string fingpayTransactionId { get; set; }
        public string stan { get; set; }
        public string bankRRN { get; set; }
        public string transactionTime { get; set; }
        public string merchantTranId { get; set; }
        public bool transactionStatus { get; set; }
        public decimal transactionAmount { get; set; }
        public string transactionStatusCode { get; set; }
        public string transactionStatusMessage { get; set; }
        public string remarks { get; set; }
        public decimal balanceAmount { get; set; }
        public string cardNumber { get; set; }
        public string aadhaarNumber { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string mobileNumber { get; set; }
        public string deviceIMEI { get; set; }
        public string bankName { get; set; }

    }

    public class FPDepositRequest
    {
        public int superMerchantId { get; set; }
        public string merchantUserName { get; set; }
        public string mobileNumber { get; set; }
        public string merchantPin { get; set; }
        //public string subMerchantId { get; set; }
        public string secretKey { get; set; }
        public string iin { get; set; }
        public string transactionType { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string requestRemarks { get; set; }
        public string merchantTranId { get; set; }
        public string accountNumber { get; set; }
        public double amount { get; set; }
        public string fingpayTransactionId { get; set; }
        public string otp { get; set; }
        public int cdPkId { get; set; }
        public string paymentType { get; set; }
    }
    public class FPDepositResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long statusCode { get; set; }
        public FPDepositData data { get; set; }
    }
    public class FPDepositData
    {
        public string fingpayTransactionId { get; set; }
        public int cdPkId { get; set; }
        public string bankRrn { get; set; }
        public string fpRrn { get; set; }
        public string stan { get; set; }
        public string merchantTranId { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string accountNumber { get; set; }
        public string mobileNumber { get; set; }
        public string beneficiaryName { get; set; }
        public string transactionTimestamp { get; set; }
    }
}
