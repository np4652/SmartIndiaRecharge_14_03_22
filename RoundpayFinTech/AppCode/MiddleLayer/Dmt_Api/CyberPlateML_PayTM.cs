using CyberPlatOpenSSL;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.CyberPlate
{
    public partial class CyberPlateML : ICyberDMTMLPayTM
    {
        private const string URLPayTMValidation = "https://in.cyberplat.com/cgi-bin/paytm/paytm_pay_check.cgi";
        private const string URLPayTMPayment = "https://in.cyberplat.com/cgi-bin/paytm/paytm_pay.cgi";
        private const string URLPayTMStatus = "https://in.cyberplat.com/cgi-bin/paytm/paytm_pay_status.cgi";
        private const string URLWalletBalance = "https://in.cyberplat.com/cgi-bin/mts_espp/mtspay_rest.cgi";
        private readonly int _APIID;
        public CyberPlateML(IDAL dal, int APIID)
        {
            _APIID = APIID;
            _dal = dal;
        }
        public ResponseStatus RemitterDetailsPTM(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " "),
                CommonInt = ErrorCodes.One,
                ErrorCode = DMTErrorCodes.Sender_Not_Found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=5");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("RemitterDetailsPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("RemitterDetailsPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.CommonInt = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                res.CommonStr = ADDINFO.limitLeft.ToString();
                                res.CommonStr2 = ADDINFO.firstName + " " + ADDINFO.lastName;
                            }

                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.One;
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus RemitterLimitPTM(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=26");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("RemitterLimitPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("RemitterLimitPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower())
                                {
                                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                                    res.CommonStr = ADDINFO.limit.ToString();
                                    res.CommonStr2 = ADDINFO.customerMobile;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus GetOTPForRegistrationPTM(DMTReq _req)
        {
            //CommonStr will hold state(Sometime called reference id)
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("RequestFor=registrationOtp");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=9");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("GetOTPForRegistrationPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        SaveDMTLog("GetOTPForRegistrationPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower())
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                    res.CommonInt = ErrorCodes.One;
                                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                    res.ReffID = ADDINFO.state;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.OTP_limit_exceeds).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_limit_exceeds;
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus RemitterRegistrationPTM(CreateSen createSen, string state)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, createSen.dMTReq.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(createSen.dMTReq.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=0");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(createSen.dMTReq.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(createSen.OTP);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("state=");
            reqBuilder.Append(state);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(createSen.senderRequest.Name ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(createSen.senderRequest.LastName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AddressName=");
            reqBuilder.Append(createSen.senderRequest.Area ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustAddress=");
            reqBuilder.Append(createSen.senderRequest.Address ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustAddress2=");
            reqBuilder.Append(createSen.senderRequest.Districtname ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("City=");
            reqBuilder.Append(createSen.senderRequest.City ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustState=");
            reqBuilder.Append(createSen.senderRequest.Statename ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Country=India");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Pin=");
            reqBuilder.Append(createSen.senderRequest.Pincode ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustMobile=");
            reqBuilder.Append(createSen.senderRequest.MobileNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("RemitterRegistrationPTM", createSen.dMTReq.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, createSen.dMTReq.SenderNO, createSen.dMTReq.UserID, createSen.dMTReq.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("RemitterRegistrationPTM", createSen.dMTReq.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, createSen.dMTReq.SenderNO, createSen.dMTReq.UserID, createSen.dMTReq.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus BeneficiaryRegistrationPTM(AddBeni addBeni, DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=4");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(addBeni.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(addBeni.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benBankName=");
            reqBuilder.Append(addBeni.BankName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benName=");
            reqBuilder.Append(addBeni.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benNickName=");
            reqBuilder.Append(addBeni.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("BeneficiaryRegistrationPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("BeneficiaryRegistrationPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                                    res.CommonStr4 = ADDINFO.beneficiaryId;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public BeniRespones ListOfBeneficiaryPTM(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=25");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("ListOfBeneficiaryPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("ListOfBeneficiaryPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    var ListBeni = new List<AddBeni>();
                                    foreach (var item in ADDINFO.beneficiaries)
                                    {
                                        var addBeni = new AddBeni
                                        {
                                            AccountNo = item.accountDetail.accountNumber,
                                            BankName = item.accountDetail.bankName,
                                            IFSC = item.accountDetail.ifscCode,
                                            BeneName = item.accountDetail.accountHolderName,
                                            MobileNo = item.accountDetail.bankName,
                                            BeneID = item.beneficiaryId
                                        };
                                        ListBeni.Add(addBeni);
                                    }
                                    res.addBeni = ListBeni;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public DMRTransactionResponse BeneAccountValidationPTM(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";

            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=10");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(sendMoney.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(sendMoney.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benBankName=");
            reqBuilder.Append(sendMoney.Bank ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLPayTMValidation + SignedReq);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            res.Response = PayTMRespV;
            SaveDMTLog("BeneAccountValidationPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);

            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    res.Request = (URLPayTMPayment + SignedReq);
                    res.Response = PayTMRespP;
                    SaveDMTLog("BeneAccountValidationPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.VendorID = PResp.TRANSID;
                            if ((PResp.TRNXSTATUS ?? string.Empty) == "7")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.BeneName = ADDINFO.extra_info.beneficiaryName;
                                    res.LiveID = ADDINFO.rrn;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public DMRTransactionResponse FundTransferPTM(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";

            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(_req.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=3");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benId=");
            reqBuilder.Append(sendMoney.BeneID ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);


            reqBuilder.Append("AMOUNT=");
            reqBuilder.Append(sendMoney.Amount);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=");
            reqBuilder.Append(sendMoney.Amount);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLPayTMValidation + SignedReq);

            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            res.Response = PayTMRespV;
            SaveDMTLog("FundTransferPTM", _req.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;

                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    res.Request = (URLPayTMPayment + SignedReq);
                    res.Response = PayTMRespP;
                    SaveDMTLog("FundTransferPTM", _req.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.VendorID = PResp.TRANSID;
                            if ((PResp.TRNXSTATUS ?? string.Empty) == "7")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.BeneName = ADDINFO.extra_info.beneficiaryName;
                                    res.LiveID = ADDINFO.rrn;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        private void RequestHelper(StringBuilder reqBuilder, string TID)
        {
            reqBuilder.Append("CERT=");
            reqBuilder.Append(CERTNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("SD=");
            reqBuilder.Append(SDCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AP=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("OP=");
            reqBuilder.Append(OPCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("SESSION=");
            reqBuilder.Append(TID.Length > 20 ? TID.Substring(0, 20) : TID);
            reqBuilder.Append(Environment.NewLine);
        }
        private CyberPalteResponse SerializeCyberPlateResponse(string CyberRespStr)
        {
            var cptResp = new CyberPalteResponse();
            if ((CyberRespStr ?? string.Empty).Contains("BEGIN") && (CyberRespStr ?? string.Empty).Contains("END"))
            {
                string Begin = "BEGIN\r\n", End = "\r\nEND";
                int start = 0, end = 0;
                start = CyberRespStr.ToString().IndexOf(Begin) + Begin.Length;
                end = CyberRespStr.ToString().IndexOf(End);
                if (end > start)
                {
                    string body = CyberRespStr.ToString().Substring(start, end - start);
                    if (body.Contains(Environment.NewLine))
                    {
                        var bodyLine = body.Split(Environment.NewLine);
                        foreach (var b in bodyLine)
                        {
                            var a = b.Split("=");
                            if (a.Length == 2)
                            {
                                switch (a[0])
                                {
                                    case nameof(cptResp.ADDINFO):
                                        cptResp.ADDINFO = a[1];
                                        break;
                                    case nameof(cptResp.AUTHCODE):
                                        cptResp.AUTHCODE = a[1];
                                        break;
                                    case nameof(cptResp.DATE):
                                        cptResp.DATE = a[1];
                                        break;
                                    case nameof(cptResp.ERRMSG):
                                        cptResp.ERRMSG = a[1];
                                        break;
                                    case nameof(cptResp.ERROR):
                                        cptResp.ERROR = a[1];
                                        break;
                                    case nameof(cptResp.RESULT):
                                        cptResp.RESULT = a[1];
                                        break;
                                    case nameof(cptResp.SESSION):
                                        cptResp.SESSION = a[1];
                                        break;
                                    case nameof(cptResp.TRANSID):
                                        cptResp.TRANSID = a[1];
                                        break;
                                    case nameof(cptResp.TRNXSTATUS):
                                        cptResp.TRNXSTATUS = a[1];
                                        break;
                                    case nameof(cptResp.OPERATOR_ERROR_MESSAGE):
                                        cptResp.OPERATOR_ERROR_MESSAGE = a[1];
                                        break;
                                    case nameof(cptResp.REST):
                                        cptResp.REST = a[1];
                                        break;
                                }
                            }

                        }
                    }
                }
            }
            return cptResp;
        }
        private void SaveDMTLog(string Method, int RMode, string Req, string Res, string SenderNo, int UserID, string TID)
        {
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = Method,
                RequestModeID = RMode,
                Request = Req,
                Response = Res,
                SenderNo = SenderNo,
                UserID = UserID,
                TID = TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        }

        public void GetCyberPlateBalance(APIBalanceResponse aPIBalanceResponse, string TID)
        {
            var reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, TID);
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            aPIBalanceResponse.Request = SignedReq;
            var cyberResp = openssl.CallCryptoAPI(SignedReq, URLWalletBalance);
            aPIBalanceResponse.Response = cyberResp;
            if (!string.IsNullOrEmpty(cyberResp) && !(cyberResp ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(cyberResp);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    if (Validate.O.IsNumeric((VResp.REST ?? string.Empty).Replace(".", ""))) {
                        aPIBalanceResponse.Balance = Convert.ToDecimal(VResp.REST);
                    }
                }
            }
        }
    }
    public partial class CyberPlateML : IMoneyTransferAPIML
    {
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Sender_Not_Found,
                IsSenderNotExists=true
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=5");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("RemitterDetailsPTM", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TransactionID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("RemitterDetailsPTM", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TransactionID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                res.RemainingLimit = ADDINFO.limitLeft;
                                res.SenderName = ADDINFO.firstName + " " + ADDINFO.lastName;
                                res.SenderMobile = request.SenderMobile;
                                res.IsSenderNotExists = false;
                            }

                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.One;
                        }
                    }
                }
            }
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=4");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(request.mBeneDetail.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(request.mBeneDetail.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benBankName=");
            reqBuilder.Append(request.mBeneDetail.BankName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benName=");
            reqBuilder.Append(request.mBeneDetail.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benNickName=");
            reqBuilder.Append(request.mBeneDetail.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("CreateBeneficiary", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TransactionID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("CreateBeneficiary", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TransactionID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                                    res.BeneID = ADDINFO.beneficiaryId;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
                {
                    Name = request.FirstName + " " + request.LastName,
                    MobileNo = request.SenderMobile,
                    Pincode = request.Pincode.ToString(),
                    Address = request.Address,
                    City = request.City,
                    StateID = request.StateID,
                    AadharNo = request.AadharNo,
                    Dob = request.DOB,
                    UserID = request.UserID
                })) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.IsOTPGenerated = true;
                    res.OTP = dbres.OTP;
                    res.WID = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = "Internal",
                Response = JsonConvert.SerializeObject(res),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            //CommonStr will hold state(Sometime called reference id)
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("RequestFor=registrationOtp");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=9");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("GetOTPForRegistrationPTM", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TransactionID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        SaveDMTLog("GetOTPForRegistrationPTM", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TransactionID);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower())
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                    res.ReferenceID = ADDINFO.state;
                                    res.IsOTPGenerated = true;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.OTP_limit_exceeds).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.OTP_limit_exceeds;
                        }
                    }
                }
            }
            return res;
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=25");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("GetBeneficiary", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TransactionID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("ListOfBeneficiaryPTM", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TransactionID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    var Beneficiaries = new List<MBeneDetail>();
                                    foreach (var item in ADDINFO.beneficiaries)
                                    {
                                        Beneficiaries.Add(new MBeneDetail
                                        {
                                            AccountNo = item.accountDetail.accountNumber,
                                            BankName = item.accountDetail.bankName,
                                            IFSC = item.accountDetail.ifscCode,
                                            BeneName = item.accountDetail.accountHolderName,
                                            MobileNo = item.accountDetail.bankName,
                                            BeneID = item.beneficiaryId
                                        });
                                    }
                                    res.Beneficiaries = Beneficiaries;
                                }
                            }
                        }
                    }
                }
            }
            return res;
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

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=0");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(request.OTP);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("state=");
            reqBuilder.Append(request.StateName);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(request.FirstName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(request.LastName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AddressName=");
            reqBuilder.Append(request.Area ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustAddress=");
            reqBuilder.Append(request.Address ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustAddress2=");
            reqBuilder.Append(request.Districtname ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("City=");
            reqBuilder.Append(request.City ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustState=");
            reqBuilder.Append(request.StateName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Country=India");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Pin=");
            reqBuilder.Append(request.Pincode.ToString());
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("CustMobile=");
            reqBuilder.Append(request.SenderMobile ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            SaveDMTLog("VerifySender", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TransactionID);
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    SaveDMTLog("VerifySender", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TransactionID);
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                    }
                }
            }
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var _res = (ResponseStatus)new ProcRemoveBeneficiaryNew(_dal).Call(new CommonReq
                {
                    CommonStr2 = request.mBeneDetail.BeneID,
                    LoginID=request.UserID
                });
                res.Statuscode = _res.Statuscode;
                res.Msg = _res.Msg;
                res.ErrorCode = _res.ErrorCode;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TID.ToString());

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=10");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(request.mBeneDetail.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(request.mBeneDetail.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benBankName=");
            reqBuilder.Append(request.mBeneDetail.BankName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLPayTMValidation + SignedReq);
            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            res.Response = PayTMRespV;
            SaveDMTLog("VerifyAccount", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TID.ToString());

            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    res.Request = (URLPayTMPayment + SignedReq);
                    res.Response = PayTMRespP;
                    SaveDMTLog("VerifyAccount", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TID.ToString());
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.VendorID = PResp.TRANSID;
                            if ((PResp.TRNXSTATUS ?? string.Empty) == "7")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.BeneName = ADDINFO.extra_info.beneficiaryName;
                                    res.LiveID = ADDINFO.rrn;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TID.ToString());

            reqBuilder.Append("AID=");
            reqBuilder.Append(request.APIOutletID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=3");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benId=");
            reqBuilder.Append(request.mBeneDetail.BeneID ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);


            reqBuilder.Append("AMOUNT=");
            reqBuilder.Append(request.Amount);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=");
            reqBuilder.Append(request.Amount);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("TERM_ID=");
            reqBuilder.Append(APCode);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLPayTMValidation + SignedReq);

            var PayTMRespV = openssl.CallCryptoAPI(SignedReq, URLPayTMValidation);
            res.Response = PayTMRespV;
            SaveDMTLog("AccountTransfer", request.RequestMode, (URLPayTMValidation + SignedReq), PayTMRespV, request.SenderMobile, request.UserID, request.TID.ToString());
            if (!string.IsNullOrEmpty(PayTMRespV) && !(PayTMRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(PayTMRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;

                    var PayTMRespP = openssl.CallCryptoAPI(SignedReq, URLPayTMPayment);
                    res.Request = (URLPayTMPayment + SignedReq);
                    res.Response = PayTMRespP;
                    SaveDMTLog("AccountTransfer", request.RequestMode, (URLPayTMPayment + SignedReq), PayTMRespP, request.SenderMobile, request.UserID, request.TID.ToString());
                    if (!string.IsNullOrEmpty(PayTMRespP) && !(PayTMRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(PayTMRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            res.VendorID = PResp.TRANSID;
                            if ((PResp.TRNXSTATUS ?? string.Empty) == "7")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.status == RechargeRespType._SUCCESS.ToLower() && ADDINFO.response_code == 0)
                                {
                                    res.BeneName = ADDINFO.extra_info.beneficiaryName;
                                    res.LiveID = ADDINFO.rrn;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
