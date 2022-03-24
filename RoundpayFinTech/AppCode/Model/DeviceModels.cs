using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.Model
{

    public class CaptureResponse
    {
        public string PidDatatype { get; set; }
        public string Piddata { get; set; }
        public string ci { get; set; }
        public string dc { get; set; }
        public string dpID { get; set; }
        public string errCode { get; set; }
        public string errInfo { get; set; }
        public string fCount { get; set; }
        public string fType { get; set; }
        public string hmac { get; set; }
        public string iCount { get; set; }
        public string iType { get; set; }
        public string mc { get; set; }
        public string mi { get; set; }
        public string nmPoints { get; set; }
        public string pCount { get; set; }
        public string pType { get; set; }
        public string qScore { get; set; }
        public string rdsID { get; set; }
        public string rdsVer { get; set; }
        public string sessionKey { get; set; }
    }
    public class CardnumberORUID
    {
        public string adhaarNumber { get; set; }
        public int indicatorforUID { get; set; }
        public string nationalBankIdentificationNumber { get; set; }
    }
    public class BalnaceEquiry
    {
        public string merchantTransactionId { get; set; }
        public CaptureResponse captureResponse { get; set; }
        public CardnumberORUID cardnumberORUID { get; set; }
        public string languageCode { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string mobileNumber { get; set; }
        public string paymentType { get; set; }
        public string requestRemarks { get; set; }
        public string timestamp { get; set; }
        public string transactionType { get; set; }
        public string merchantUserName { get; set; }
        public string merchantPin { get; set; }
        //public string subMerchantId { get; set; }
        public string superMerchantId { get; set; }
        public string transactionAmount { get; set; }
    }
    public class WithDrawEquiry
    {
        public string merchantTranId { get; set; }
        public CaptureResponse captureResponse { get; set; }
        public CardnumberORUID cardnumberORUID { get; set; }
        public string languageCode { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string mobileNumber { get; set; }
        public string paymentType { get; set; }
        public string requestRemarks { get; set; }
        public string timestamp { get; set; }
        public string transactionType { get; set; }
        public string merchantUserName { get; set; }
        public string merchantPin { get; set; }
        //public string subMerchantId { get; set; }
        public string superMerchantId { get; set; }
        public string transactionAmount { get; set; }
    }

    public class BalanceEquiryResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public double Balance { get; set; }
        public string BankRRN { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
    }
    public class WithdrawlResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public double Balance { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public string TransactionID { get; set; }
        public int Status { get; set; }
        public int Errorcode { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
    }

    public class DepositRequest
    {
        public string AccountNo { get; set; }
        public string IIN { get; set; }
        public int InterfaceType { get; set; }
        public int Amount { get; set; }
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int RMode { get; set; }
        public int OID { get; set; }
        public bool IsGetBene { get; set; }
        public string OTP { get; set; }
        public string Reff1 { get; set; }
        public string Reff2 { get; set; }
        public string Reff3 { get; set; }
        public string IMEI { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
    }
    public class DepositResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public double Balance { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public string TransactionID { get; set; }
        public string RefferenceNo { get; set; }
        public int Status { get; set; }
        public int Errorcode { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string BeneficaryName { get; set; }
    }
    public class CashWithdrawal
    {
        public string merchantTranId { get; set; }
        public CaptureResponse captureResponse { get; set; }
        public CardnumberORUID cardnumberORUID { get; set; }
        public string languageCode { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string mobileNumber { get; set; }
        public string paymentType { get; set; }
        public string requestRemarks { get; set; }
        public string timestamp { get; set; }
        public int transactionAmount { get; set; }
        public string transactionType { get; set; }
        public string merchantUserName { get; set; }
        public string merchantPin { get; set; }
        public string subMerchantId { get; set; }
    }

    public class MiniStatementResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int Status { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string LiveID { get; set; }
        public double Balance { get; set; }
        public string VendorID { get; set; }
        public List<MiniStatement> Statements { get; set; }
    }
    public class MiniStatement
    {
        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Amount { get; set; }
        public string Narration { get; set; }
    }

    [XmlRoot(ElementName = "Resp")]
    public class Resp
    {
        [XmlAttribute(AttributeName = "errCode")]
        public string ErrCode { get; set; }
        [XmlAttribute(AttributeName = "errInfo")]
        public string ErrInfo { get; set; }
        [XmlAttribute(AttributeName = "fCount")]
        public string FCount { get; set; }
        [XmlAttribute(AttributeName = "fType")]
        public string FType { get; set; }
        [XmlAttribute(AttributeName = "nmPoints")]
        public string NmPoints { get; set; }
        [XmlAttribute(AttributeName = "qScore")]
        public string QScore { get; set; }
    }

    [XmlRoot(ElementName = "Param")]
    public class Param
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "additional_info")]
    public class Additional_info
    {
        [XmlElement(ElementName = "Param")]
        public List<Param> Param { get; set; }
    }

    [XmlRoot(ElementName = "DeviceInfo")]
    public class DeviceInfo
    {
        [XmlElement(ElementName = "additional_info")]
        public Additional_info additionalInfo { get; set; }
        [XmlAttribute(AttributeName = "dpId")]
        public string DpId { get; set; }
        [XmlAttribute(AttributeName = "rdsId")]
        public string RdsId { get; set; }
        [XmlAttribute(AttributeName = "rdsVer")]
        public string RdsVer { get; set; }
        [XmlAttribute(AttributeName = "mi")]
        public string Mi { get; set; }
        [XmlAttribute(AttributeName = "mc")]
        public string Mc { get; set; }
        [XmlAttribute(AttributeName = "dc")]
        public string Dc { get; set; }
    }

    [XmlRoot(ElementName = "Skey")]
    public class Skey
    {
        [XmlAttribute(AttributeName = "ci")]
        public string Ci { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Data")]
    public class Data
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "PidData")]
    public class PidData
    {
        [XmlElement(ElementName = "Resp")]
        public Resp Resp { get; set; }
        [XmlElement(ElementName = "DeviceInfo")]
        public DeviceInfo DeviceInfo { get; set; }
        [XmlElement(ElementName = "Skey")]
        public Skey Skey { get; set; }
        [XmlElement(ElementName = "Hmac")]
        public string Hmac { get; set; }
        [XmlElement(ElementName = "Data")]
        public Data Data { get; set; }
    }


    public class DeviceResponse
    {
        public bool status { get; set; }
        public string message { get; set; }

        public DeviceResponseData data { get; set; }
        public int statusCode { get; set; }
    }
    public class DeviceResponseData
    {
        public string terminalId { get; set; }
        public string requestTransactionTime { get; set; }
        public double transactionAmount { get; set; }
        public string transactionStatus { get; set; }
        public string transactionStatusCode { get; set; }
        public double balanceAmount { get; set; }
        public string bankRRN { get; set; }
        public string transactionType { get; set; }
        public string fpTransactionId { get; set; }
        public object merchantTxnId { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public object merchantTransactionId { get; set; }
        public object bankAccountNumber { get; set; }
        public object ifscCode { get; set; }
        public object bcName { get; set; }
        public object transactionTime { get; set; }
        public int agentId { get; set; }
        public object issuerBank { get; set; }
        public object customerAadhaarNumber { get; set; }
        public object customerName { get; set; }
        public object stan { get; set; }
        public object rrn { get; set; }
        public object uidaiAuthCode { get; set; }
        public object bcLocation { get; set; }
        public object demandSheetId { get; set; }
        public object mobileNumber { get; set; }
        public object urnId { get; set; }
        public object transactionRemark { get; set; }
        public object bankName { get; set; }
        public object prospectNumber { get; set; }
        public object internalReferenceNumber { get; set; }
        public object biTxnType { get; set; }
        public object subVillageName { get; set; }
        public object userProfileResponseModel { get; set; }
        public List<MiniSTMTStructureModel> miniStatementStructureModel { get; set; }
    }

    public class MiniSTMTStructureModel
    {
        public string date { get; set; }
        public string txnType { get; set; }
        public string amount { get; set; }
        public string narration { get; set; }
    }
}
