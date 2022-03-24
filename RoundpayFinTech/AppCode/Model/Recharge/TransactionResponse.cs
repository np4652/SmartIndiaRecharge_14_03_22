using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    #region TransactionResponse
    public class StatusMsg
    {
        private string _blank = " ";
        private string _mSG;
        private string _eCode;
        public int STATUS { get; set; }
        public string MSG { get => string.IsNullOrEmpty(_mSG) ? _blank : _mSG; set => _mSG = value; }
        public decimal BAL { get; set; }
        public string ERRORCODE { get => string.IsNullOrEmpty(_eCode) ? _blank : _eCode; set => _eCode = value; }
        public string Operator { get; set; }
        public bool IsOTPRequired { get; set; }
        public bool IsShow { get; set; }

        public bool ShouldSerializeOperator() => false;
        public bool ShouldSerializeIsOTPRequired() => IsShow;
        public bool ShouldSerializeIsShow() => false;
    }
    public class BillerAPIResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ErrorCode { get; set; }
        public RPBillerModel data { get; set; }
    }
    public class TransactionResponse : StatusMsg
    {
        private string _blank = " ";
        private string _aCCOUNT;
        private string _rPID;
        private string _aGENTID;
        private string _oPID;
        private string _customerName;
        private string _dueDate;
        private string _billDate;
        private string _billNumber;
        private string _billPeriod;
        private string _refID;

        public string ACCOUNT { get => ((_aCCOUNT ?? string.Empty) == string.Empty ? _blank : _aCCOUNT); set => _aCCOUNT = value; }
        public decimal AMOUNT { get; set; }
        public string RPID { get => ((_rPID ?? string.Empty) == string.Empty ? _blank : _rPID); set => _rPID = value; }
        public string AGENTID { get => ((_aGENTID ?? string.Empty) == string.Empty ? _blank : _aGENTID); set => _aGENTID = value; }
        public string OPID { get => ((_oPID ?? string.Empty) == string.Empty ? _blank : _oPID); set => _oPID = value; }
        public decimal DUEAMOUNT { get; set; }
        public string CUSTOMERNAME { get => ((_customerName ?? string.Empty) == string.Empty ? _blank : _customerName); set => _customerName = value; }
        public string DUEDATE { get => ((_dueDate ?? string.Empty) == string.Empty ? _blank : _dueDate); set => _dueDate = value; }
        public string BILLDATE { get => ((_billDate ?? string.Empty) == string.Empty ? _blank : _billDate); set => _billDate = value; }
        public string BILLNUMBER { get => ((_billNumber ?? string.Empty) == string.Empty ? _blank : _billNumber); set => _billNumber = value; }
        public string BILPERIOD { get => ((_billPeriod ?? string.Empty) == string.Empty ? _blank : _billPeriod); set => _billPeriod = value; }
        public string REFID { get => ((_refID ?? string.Empty) == string.Empty ? _blank : _refID); set => _refID = value; }


        public bool IsBillFetch { get; set; }
        public bool IsBBPS { get; set; }

        public int FetchBillID { get; set; }
        public bool ShouldSerializeFetchBillID() => (IsBillFetch);
        public bool ShouldSerializeDUEAMOUNT() => (IsBillFetch || IsBBPS);
        public bool ShouldSerializeCUSTOMERNAME() => (IsBillFetch);
        public bool ShouldSerializeDUEDATE() => (IsBillFetch);
        public bool ShouldSerializeBILLDATE() => (IsBillFetch);
        public bool ShouldSerializeBILLNUMBER() => (IsBillFetch);
        public bool ShouldSerializeBILPERIOD() => (IsBillFetch);
        public bool ShouldSerializeREFID() => (IsBillFetch && STATUS != RechargeRespType.FAILED);

        public bool ShouldSerializeIsBillFetch() => (false);
        public bool ShouldSerializeIsBBPS() => (false);

        public int ServiceID { get; set; }
        public int APIID { get; set; }
        public string APICode { get; set; }

        public bool ShouldSerializeServiceID() => (false);
        public bool ShouldSerializeAPIID() => (false);
        public bool ShouldSerializeAPICode() => (false);

        public bool IsRefundable { get; set; }
        public bool ShouldSerializeIsRefundable() => (ServiceID == 9 && !IsRefunded);

        public string VendorID { get; set; }
        public bool ShouldSerializeVendorID() => (false);

        public string Description { get; set; }
        public bool ShouldSerializeDescription() => (false);

        public int TID { get; set; }
        public bool ShouldSerializeTID() => (false);

        public bool IsRefunded { get; set; }
        public bool ShouldSerializeIsRefunded() => (ServiceID == 9);

        public bool IsRefundStatusShow { get; set; }
        public Int16 RefundStatus { get; set; }
        public bool ShouldSerializeRefundStatus() => IsRefundStatusShow;
        public bool ShouldSerializeRefundStatusShow() => false;
    }

    public class DMTCheckStatusResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public int ErrorCode { get; set; }
        public string Account { get; set; }
        public int Amount { get; set; }
        public decimal Balance { get; set; }
        public string AgentID { get; set; }
        public string TransactionID { get; set; }
        public string LiveID { get; set; }
        public string BeneName { get; set; }
        public bool IsRefundStatusShow { get; set; }
        public Int16 RefundStatus { get; set; }
        public bool ShouldSerializeRefundStatus() => IsRefundStatusShow;
    }

    #endregion

    #region OutletResponseSection
    public class OutletStatusRes
    {
        private readonly string _blank = " ";
        private string _sTATUS;
        private string _mSG;
        [XmlElement("STATUS")]
        [JsonProperty("STATUS")]
        public string STATUS { get => _sTATUS ?? _blank; set => _sTATUS = value; }
        [XmlElement("MSG")]
        [JsonProperty("MSG")]
        public string MSG { get => _mSG ?? _blank; set => _mSG = value; }
    }
    public class OutletInfo : OutletStatusRes
    {
        private readonly string _blank = " ";
        private string _name;
        private string _company;
        private string _mobileNo;
        private string _emailID;
        private string _pincode;
        private string _address;
        private string _pAN;
        private string _aADHAR;
        private string _kYCStatus;
        private string _verifyStatus;
        private string _activeStatus;
        private string _oType;

        public int OutletID { get; set; }
        public string Name { get => (_name ?? "") == "" ? _blank : _name; set => _name = value; }
        public string Company { get => (_company ?? "") == "" ? _blank : _company; set => _company = value; }
        public string MobileNo { get => (_mobileNo ?? "") == "" ? _blank : _mobileNo; set => _mobileNo = value; }
        public string EmailID { get => (_emailID ?? "") == "" ? _blank : _emailID; set => _emailID = value; }
        public string Pincode { get => (_pincode ?? "") == "" ? _blank : _pincode; set => _pincode = value; }
        public string Address { get => (_address ?? "") == "" ? _blank : _address; set => _address = value; }
        public string PAN { get => (_pAN ?? "") == "" ? _blank : _pAN; set => _pAN = value; }
        public string AADHAR { get => (_aADHAR ?? "") == "" ? _blank : _aADHAR; set => _aADHAR = value; }
        public string KYCStatus { get => (_kYCStatus ?? "") == "" ? _blank : _kYCStatus; set => _kYCStatus = value; }
        public string VerifyStatus { get => (_verifyStatus ?? "") == "" ? _blank : _verifyStatus; set => _verifyStatus = value; }
        public string ActiveStatus { get => (_activeStatus ?? "") == "" ? _blank : _activeStatus; set => _activeStatus = value; }
        public string OType { get => (_oType ?? "") == "" ? _blank : _oType; set => _oType = value; }
        public string RefID { get; set; }
        public string OTPStatus { get; set; }

        public bool ShouldSerializeOutletID() => (OutletID > 0);
        public bool ShouldSerializeName() => (OutletID > 0);
        public bool ShouldSerializeCompany() => (OutletID > 0);
        public bool ShouldSerializeEmailID() => (OutletID > 0);
        public bool ShouldSerializePincode() => (OutletID > 0);
        public bool ShouldSerializeAddress() => (OutletID > 0);
        public bool ShouldSerializePAN() => (OutletID > 0);
        public bool ShouldSerializeAADHAR() => (OutletID > 0);
        public bool ShouldSerializeVerifyStatus() => (OutletID > 0);
        public bool ShouldSerializeActiveStatus() => (OutletID > 0);
        public bool ShouldSerializeOType() => (OutletID > 0);
        public bool ShouldSerializeKYCStatus() => (OutletID > 0);

        public bool ShouldSerializeRefID() => ((RefID ?? "") != "");
        public bool ShouldSerializeOTPStatus() => ((OTPStatus ?? "").Trim() != "");
    }
    public class OutletStatusResponse : OutletStatusRes
    {
        private readonly string _blank = " ";
        private string _mobileNo;
        public string MobileNo { get => (_mobileNo ?? "") == "" ? _blank : _mobileNo; set => _mobileNo = value; }
        public string RefID { get; set; }
        public int OutletID { get; set; }
        public string OTPStatus { get; set; }
        public bool ShouldSerializeOutletID() => (OutletID > 0);
        public bool ShouldSerializeOTPStatus() => ((OTPStatus ?? "").Trim() != "");
    }
    public class KYCInfo : OutletStatusRes
    {
        public List<KYCDoc> KYCDocs { get; set; }
    }
    public class KYCDoc
    {
        public int DocTypeID { get; set; }
        public string DocName { get; set; }
        public string DOCURL { get; set; }
        public bool IsMandatory { get; set; }
        public string VerifyStatus { get; set; }

        public bool ShouldSerializeDocTypeID() => (DocTypeID > 0);
        public bool ShouldSerializeDocName() => (DocTypeID > 0);
        public bool ShouldSerializeIsMandatory() => (DocTypeID > 0);
        public bool ShouldSerializeVerifyStatus() => (DocTypeID > 0);
    }
    #endregion

    #region MoneyTransferResponses
    public class MTStatusRes
    {
        private readonly string _blank = " ";
        private string _sTATUS;
        private string _mSG;
        private string _oTPStatus;
        [XmlElement("STATUS")]
        [JsonProperty("STATUS")]
        public string STATUS { get => ((_sTATUS ?? "") == "" ? _blank : _sTATUS); set => _sTATUS = value; }
        [XmlElement("MSG")]
        [JsonProperty("MSG")]
        public string MSG { get => ((_mSG ?? "") == "" ? _blank : _mSG); set => _mSG = value; }

        public string OTPStatus { get => _oTPStatus ?? _blank; set => _oTPStatus = value; }
        public bool ShouldSerializeOTPStatus() => ((OTPStatus ?? "").Trim() != "");
    }

    public class SenderInfoResponse : MTStatusRes
    {
        private readonly string _blank = " ";
        private string _pincode;
        private string _verifySts;

        public string SenderNo { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string Pincode { get => _pincode ?? _blank; set => _pincode = value; }
        public bool ShouldSerializeSenderNo() => (!string.IsNullOrEmpty(SenderNo));
        public bool ShouldSerializeName() => (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(SenderNo));
        public bool ShouldSerializeMobileNo() => (!string.IsNullOrEmpty(MobileNo) && !string.IsNullOrEmpty(SenderNo));
        public bool ShouldSerializePincode() => (!string.IsNullOrEmpty(Pincode) && !string.IsNullOrEmpty(SenderNo));

        public string KYCStatus { get; set; }
        public bool ShouldSerializeKYCStatus() => ((KYCStatus ?? "") != "");

        public decimal TotalLimit { get; set; }
        public decimal AvailableLimit { get; set; }
        public string VerifyStatus { get => _verifySts ?? _blank; set => _verifySts = value; }

        public bool ShouldSerializeTotalLimit() => ((STATUS ?? RechargeRespType._FAILED) != RechargeRespType._FAILED);
        public bool ShouldSerializeAvailableLimit() => ((STATUS ?? RechargeRespType._FAILED) != RechargeRespType._FAILED);
        public bool ShouldSerializeVerifyStatus() => ((STATUS ?? RechargeRespType._FAILED) != RechargeRespType._FAILED);
    }
    public class BeneficiaryListReponse : MTStatusRes
    {
        public List<Beneficiery> Beneficiaries { get; set; }
    }
    public class Beneficiery
    {
        private readonly string _blank = " ";
        private string _beneName;
        private string _accountNo;
        private string _iFSC;
        private string _banakName;
        private string _beneID;

        public string BeneID { get => _beneID ?? _blank; set => _beneID = value; }
        public string BeneName { get => _beneName ?? _blank; set => _beneName = value; }
        public string AccountNo { get => _accountNo ?? _blank; set => _accountNo = value; }
        public string IFSC { get => _iFSC ?? _blank; set => _iFSC = value; }
        public string BankName { get => _banakName ?? _blank; set => _banakName = value; }
    }
    public class BeneIDAccount
    {
        public string BeneID { get; set; }
        public string Account { get; set; }
    }
    public class BeneficieryResponse : MTStatusRes
    {
        private readonly string _blank = " ";
        private string _beneName;
        private string _accountNo;
        private string _iFSC;
        private string _banakName;
        private string _beneID;

        public string BeneID { get => _beneID ?? _blank; set => _beneID = value; }
        public string BeneName { get => _beneName ?? _blank; set => _beneName = value; }
        public string AccountNo { get => _accountNo ?? _blank; set => _accountNo = value; }
        public string IFSC { get => _iFSC ?? _blank; set => _iFSC = value; }
        public string BanakName { get => _banakName ?? _blank; set => _banakName = value; }
    }

    public class DMTResponse : MTStatusRes
    {
        private readonly string _blank = " ";
        private string _aCCOUNT;
        private string _beneName;
        private string _rPID;
        private string _aGENTID;
        private string _oPID;
        [XmlElement("BAL")]
        [JsonProperty("BAL")]
        public decimal BAL { get; set; }
        [XmlElement("ACCOUNT")]
        [JsonProperty("ACCOUNT")]
        public string ACCOUNT { get => ((_aCCOUNT ?? "") == "" ? _blank : _aCCOUNT); set => _aCCOUNT = value; }
        [XmlElement("AMOUNT")]
        [JsonProperty("AMOUNT")]
        public decimal AMOUNT { get; set; }



        [XmlElement("RPID")]
        [JsonProperty("RPID")]
        public string RPID { get => ((_rPID ?? "") == "" ? _blank : _rPID); set => _rPID = value; }
        [XmlElement("AGENTID")]
        [JsonProperty("AGENTID")]
        public string AGENTID { get => ((_aGENTID ?? "") == "" ? _blank : _aGENTID); set => _aGENTID = value; }
        [XmlElement("OPID")]
        [JsonProperty("OPID")]
        public string OPID { get => ((_oPID ?? "") == "" ? _blank : _oPID); set => _oPID = value; }

        public string BeneficiaryName { get; set; }
        public bool ShouldSerializeBeneficiaryName() => ((BeneficiaryName ?? "").Trim() != "");

        public string IFSC { get; set; }
        public bool ShouldSerializeIFSC() => ((IFSC ?? "").Trim() != "");
    }

    public class DMTStatusResponse
    {
        public string Request { get; set; }
        public string Response { get; set; }
        public bool RefundStatus { get; set; }
        public string MSG { get; set; }
        public string _DMRStatus { get; set; }
        public int DMRStatus { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
    }
    #endregion

    public class TransactionServiceReq
    {
        public bool IsPostpaid { get; set; }
        public int OID { get; set; }
        public int CircleID { get; set; }
        public int UserID { get; set; }
        public string AccountNo { get; set; }
        public decimal AmountR { get; set; }
        public string OPID { get; set; }
        public string APIRequestID { get; set; }
        public int RequestModeID { get; set; }
        public string RequestIP { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }
        public string Extra { get; set; }
        public int OutletID { get; set; }
        public string RefID { get; set; }
        public string GEOCode { get; set; }
        public string CustomerNumber { get; set; }
        public string PinCode { get; set; }
        public string IMEI { get; set; }
        public string Browser { get; set; }
        public string SecurityKey { get; set; }
        public string PAN { get; set; }
        public string Aadhar { get; set; }
        public bool IsReal { get; set; }
        public int PromoCodeID { get; set; }
        public int FetchBillID { get; set; }
        public int CCFAmount { get; set; }
        public string PaymentMode { get; set; }
        public bool IsStatusCheck { get; set; }
        public string TransactionReqID { get; set; }
        public string VendorID { get; set; }
        public string APIContext { get; set; }
        public int TID { get; set; }
    }
    public class TransactionStatus
    {
        public int Status { get; set; }
        public string VendorID { get; set; }
        public string OperatorID { get; set; }
        public int APIID { get; set; }
        public int APIType { get; set; }
        public int UserID { get; set; }
        public int TID { get; set; }
        public string APIOpCode { get; set; }
        public string APIName { get; set; }
        public bool APICommType { get; set; }
        public decimal APIComAmt { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ExtraParam { get; set; }
        public string APIGroupCode { get; set; }
        public string APIErrorCode { get; set; }
        public string APIMsg { get; set; }
        public string RefferenceID { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public bool IsResend { get; set; }
        public int SwitchingID { get; set; }
    }
    public class TransactionServiceResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int TID { get; set; }
        public int WID { get; set; }
        public string TransactionID { get; set; }
        public APIDetail CurrentAPI { get; set; }
        public List<APIDetail> MoreAPIs { get; set; }
        public decimal Balance { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsAmountInto100 { get; set; }
        public string OutletMobile { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string GeoCode { get; set; }
        public string Pincode { get; set; }
        public int TotalToken { get; set; }
        public string AccountNo { get; set; }
        public string OPID { get; set; }
        public int ErrorCode { get; set; }
        public bool IsPartialPay { get; set; }
        public string CircleCode { get; set; }
        public string PAN { get; set; }
        public string Aadhar { get; set; }
        public string BillerID { get; set; }
        public bool IsINT { get; set; }
        public bool IsMOB { get; set; }
        public bool BillerAdhoc { get; set; }
        public int ExactNess { get; set; }
        public string APIOpType { get; set; }
        public string REFID { get; set; }
        public string Operator { get; set; }
        public string LiveID { get; set; }
        public bool IsManual { get; set; }
        public bool IsBilling { get; set; }
        public bool IsBillValidation { get; set; }
        public string InitChanel { get; set; }
        public string MAC { get; set; }
        public string PaymentMode { get; set; }
        public string BillDate { get; set; }
        public string EarlyPaymentDate { get; set; }
        public string DueDate { get; set; }
        public string PaymentModeInAPI { get; set; }
        public string CaptureInfo { get; set; }
        public string UserMobileNo { get; set; }
        public string UserName { get; set; }
        public string UserEmailID { get; set; }
        public string AccountNoKey { get; set; }
        public string APIContext { get; set; }
        public string AccountHolder { get; set; }
        public string RegxAccount { get; set; }
        public string EarlyPaymentAmountKey { get; set; }
        public string LatePaymentAmountKey { get; set; }
        public string EarlyPaymentDateKey { get; set; }
        public string BillFetchResponse { get; set; }
        public string BillMonthKey { get; set; }
        public string BillerPaymentModes { get; set; }
        public List<OperatorParams> OpParams { get; set; }
        public List<OperatorOptionalDictionary> OpOptionalDic { get; set; }
        public List<TranAdditionalInfo> AInfos { get; set; }
    }
    public class OperatorOptionalDictionary
    {
        public int OptionalID { get; set; }
        public string Value { get; set; }
    }
    public class TranAdditionalInfo
    {
        public string InfoName { get; set; }
        public string InfoValue { get; set; }
    }
    public class OperatorParams
    {
        public int Ind { get; set; }
        public string Param { get; set; }
        public string DataType { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string RegEx { get; set; }
        public string Remark { get; set; }
        public bool IsAccountNo { get; set; }
        public bool IsOptional { get; set; }
        public bool IsCustomerNo { get; set; }
        public bool IsDropDown { get; set; }
        public int OptionalID { get; set; }

        public bool IsErrorFound { get; set; }
        public string FormatedError { get; set; }

        public void GetFormatedError(string RequestKey, string RequestValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Please enter a valid {ParamKey} .");
            sb.Append(RequestKey ?? string.Empty);
            sb.Append(" [");
            sb.Append(Param ?? string.Empty);
            sb.Append("].");
            if (!string.IsNullOrEmpty(RegEx))
            {
                IsErrorFound = !Regex.IsMatch(RequestValue, RegEx);
            }
            if (MinLength > 0 && MaxLength > 0)
            {
                if (MinLength == MaxLength)
                {
                    sb.Append("Length should be ");
                    sb.Append(MinLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length != MinLength : IsErrorFound;
                }
                else
                {
                    sb.Append("Length should be between ");
                    sb.Append(MinLength);
                    sb.Append(" and ");
                    sb.Append(MaxLength);

                    IsErrorFound = IsErrorFound == false ? RequestValue.Length < MinLength || RequestValue.Length > MaxLength : IsErrorFound;
                }
            }
            else
            {
                if (MinLength > 0)
                {
                    sb.Append("Length is greater than or equal to ");
                    sb.Append(MinLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length < MinLength : IsErrorFound;
                }
                if (MaxLength > 0)
                {
                    sb.Append("Length is less than or equal to ");
                    sb.Append(MaxLength);
                    IsErrorFound = IsErrorFound == false ? RequestValue.Length > MaxLength : IsErrorFound;
                }
            }
            if (!string.IsNullOrEmpty(this.DataType))
            {
                if (this.DataType.ToUpper().Equals("NUMERIC"))
                {
                    sb.Append(" digit.");
                    IsErrorFound = IsErrorFound == false ? !Validators.Validate.O.IsNumeric(RequestValue) : IsErrorFound;
                }
                else
                {
                    sb.Append(" ");
                    sb.Append(this.DataType);
                    sb.Append(" characters.");
                    IsErrorFound = IsErrorFound == false ? !Validators.Validate.O.IsAlphaNumeric(RequestValue) && !RequestValue.Contains("-") && !RequestValue.Contains("_") && !RequestValue.Contains("/") : IsErrorFound;
                }
            }
            if (!string.IsNullOrEmpty(RegEx))
            {
                sb.Append("Refer regular expression ");
                sb.Append(RegEx);
            }
            FormatedError = sb.ToString();
        }
    }
    public class RechargeAPIHit
    {
        public JRRechargeReq JRRechReq { get; set; }
        public APIDetail aPIDetail { get; set; }
        public string Response { get; set; }
        public bool IsException { get; set; }
        public int LoginID { get; set; }
        public int ServiceID { get; set; }
        public CyberPlatRequestModel CPTRNXRequest { get; set; }
        public string SessionNo { get; set; }
        public string MGPSARequestID { get; set; }
    }

    public class PSAResponse
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public string Req { get; set; }
        public string Resp { get; set; }
        public string LiveID { get; set; }
        public string VendorID { get; set; }
        public int ErrorCode { get; set; }
    }
    public class APIBalanceResponse
    {
        public int Statuscode { get; set; }
        public decimal Balance { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string Template { get; set; }
    }

    #region PartnerRelatedAPIs
    public class GenerateURLResponse
    {
        public int Statuscode { get; set; }
        public int Errorcode { get; set; }
        public string Msg { get; set; }
        public string URLSession { get; set; }
        public string RedirectURL { get; set; }
    }
    public class AEPSURLSessionResp
    {
        public int APIUserID { get; set; }
        public int OutletID { get; set; }
        public int PartnerID { get; set; }
        public int URLID { get; set; }
        public int t { get; set; }
        public string TransactionID { get; set; }
    }
    #endregion
}
