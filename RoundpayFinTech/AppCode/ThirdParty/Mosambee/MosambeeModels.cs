using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Permissions;

namespace RoundpayFinTech.AppCode.ThirdParty.Mosambee
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class EmvData
    {
        [JsonProperty("91")]
        public string _91 { get; set; }
        [JsonProperty("8A")]
        public string _8A { get; set; }
    }

    public class MosambeeRoot
    {
        public string date { get; set; }
        public string approvalCode { get; set; }
        public string appVersion { get; set; }
        public string businessName { get; set; }
        public string retrievalReferenceNumber { get; set; }
        public int deviceId { get; set; }
        public string DES3Key { get; set; }
        public string responseCode { get; set; }
        public string issuerCTI { get; set; }
        [JsonProperty("9F12")]
        public string _9F12 { get; set; }
        public string merchantId { get; set; }
        [JsonProperty("9F11")]
        public string _9F11 { get; set; }
        public string acquirerName { get; set; }
        public string batchNumber { get; set; }
        public string longitude { get; set; }
        public string ac { get; set; }
        public string receivingInstitutionCode { get; set; }
        public string transactionId { get; set; }
        public string name { get; set; }
        public string tvr { get; set; }
        public string tipOption { get; set; }
        public string tgTransactionID { get; set; }
        public string creditDebitCardType { get; set; }
        public string billNumber { get; set; }
        public string applicationId { get; set; }
        public string cardNumber { get; set; }
        public string statusCode { get; set; }
        public string cid { get; set; }
        public string latitude { get; set; }
        public string terminalId { get; set; }
        public string tsi { get; set; }
        public string result { get; set; }
        public string expiryDate { get; set; }
        public string appLabel { get; set; }
        public string invoiceNumber { get; set; }
        public string cashBackAmount { get; set; }
        public string currency { get; set; }
        public string tgName { get; set; }
        public string cashBack { get; set; }
        public string amount { get; set; }
        public string cardHolderName { get; set; }
        public string address2 { get; set; }
        public string tipAmount { get; set; }
        public string transactionMode { get; set; }
        public string address1 { get; set; }
        public string cardType { get; set; }
        [JsonProperty("9F06")]
        public string _9F06 { get; set; }
        public string sessionId { get; set; }
        public string message { get; set; }
        public EmvData emvData { get; set; }
        public string balanceAmount { get; set; }
        public int transactionType { get; set; }
        public int transactionTypeId { get; set; }
        public bool isPinVerified { get; set; }
        public string stan { get; set; }
        public string time { get; set; }
        public string transactionTypeName { get; set; }



        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string transactionDate { get; set; }
        public string transactionTime { get; set; }
        public string transactionLat { get; set; }
        public string transactionLong { get; set; }
        public string transactionSTAN { get; set; }
        public string transactionRRN { get; set; }
        public string transactionAmount { get; set; }
        public string transactionAuthCode { get; set; }
        public string transactionBatchNumber { get; set; }
        public string transactionCardNumber { get; set; }
        public string transactionTerminalId { get; set; }
        public string transactionStatus { get; set; }
        public string transactionID { get; set; }

        public string currencyId { get; set; }
        public string narration { get; set; }
        public string aid { get; set; }
        public string apn { get; set; }
        public string tipProcessing { get; set; }
        public bool MsrAndPinVerification { get; set; }
        public string cvm { get; set; }
        public string refTxnId { get; set; }
        public string tgTransactionId { get; set; }
        public string checksum { get; set; }

    }


}
