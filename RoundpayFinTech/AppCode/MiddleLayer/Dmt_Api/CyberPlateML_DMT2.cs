using CyberPlatOpenSSL;
using Fintech.AppCode.DB;
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
    public partial class CyberPlateML : ICyberDMTMLIPay
    {
        private const string URLIPayValidation = "https://in.cyberplat.com/cgi-bin/instp/instp_pay_check.cgi";
        private const string URLIPayPayment = "https://in.cyberplat.com/cgi-bin/instp/instp_pay.cgi";
        private const string URLIPayStatus = "https://in.cyberplat.com/cgi-bin/instp/instp_pay_status.cgi";

        public BeniRespones RemitterDetailsIPay(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " "),
                CommonInt = ErrorCodes.One,
                ErrorCode = DMTErrorCodes.Sender_Not_Found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=5");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemitterDetailsIPay", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemitterDetailsIPay", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        PResp.ERROR = PResp.ERROR ?? "-1";
                        if (Convert.ToInt16(PResp.ERROR) == 0)
                        {
                            res.CommonInt = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if ((ADDINFO.statuscode ?? string.Empty) == "TXN")
                                {
                                    res.ReffID = ADDINFO.data.remitter.id;
                                    var ListBeni = new List<AddBeni>();
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        foreach (var item in ADDINFO.data.beneficiary)
                                        {
                                            var addBeni = new AddBeni
                                            {
                                                AccountNo = item.account,
                                                BankName = item.bank,
                                                IFSC = item.ifsc,
                                                BeneName = item.name,
                                                MobileNo = item.mobile,
                                                BeneID = item.id
                                            };
                                            ListBeni.Add(addBeni);
                                        }
                                        res.addBeni = ListBeni;
                                        res.CommonStr = ADDINFO.data.remitter.remaininglimit.ToString();
                                        res.CommonStr2 = ADDINFO.data.remitter.name;
                                    }
                                    else
                                    {
                                        res.Statuscode = ErrorCodes.Minus1;
                                        res.CommonInt = ErrorCodes.One;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public ResponseStatus RemitterRegistrationIPay(CreateSen createSen)
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

            reqBuilder.Append("Type=0");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(createSen.dMTReq.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(createSen.senderRequest.LastName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(createSen.senderRequest.Name ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Pin=");
            reqBuilder.Append(createSen.senderRequest.Pincode ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemitterRegistrationIPay", createSen.dMTReq.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, createSen.dMTReq.SenderNO, createSen.dMTReq.UserID, createSen.dMTReq.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemitterRegistrationIPay", createSen.dMTReq.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, createSen.dMTReq.SenderNO, createSen.dMTReq.UserID, createSen.dMTReq.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                        res.ReffID = ADDINFO.data.remitter.id;
                                    }
                                    else
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                        res.CommonInt = ErrorCodes.One;
                                        res.ReffID = ADDINFO.data.remitter.id;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public ResponseStatus RemitterOTPVerifyIPay(DMTReq _req, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=24");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(_req.ReffID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(OTP);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemitterOTPVerifyIPay", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemitterOTPVerifyIPay", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                        res.ReffID = ADDINFO.data.remitter.id;
                                    }
                                    else
                                    {
                                        res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Invalid_OTP;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus BeneficiaryRegistration(AddBeni addBeni, DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=4");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(_req.ReffID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(addBeni.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(addBeni.BeneName);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(addBeni.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(addBeni.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("BeneficiaryRegistration", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("BeneficiaryRegistration", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPay>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                                    res.CommonStr4 = ADDINFO.data.beneficiary.id;
                                    res.ReffID = ADDINFO.data.remitter.id;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public DMRTransactionResponse BeneAccountValidationIPay(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";

            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=10");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(sendMoney.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(sendMoney.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLIPayValidation + SignedReq);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            res.Response = IPayRespV;
            SaveDMTLog("BeneAccountValidationPTM", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);

            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    res.Request = (URLIPayPayment + SignedReq);
                    res.Response = IPayRespP;
                    SaveDMTLog("BeneAccountValidationPTM", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
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
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.BeneName = ADDINFO.data.benename;
                                    res.LiveID = ADDINFO.data.bankrefno;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public DMRTransactionResponse FundTransferIPay(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";

            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=3");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(_req.SenderNO);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("routingType=");
            reqBuilder.Append(sendMoney.Channel ? "IMPS" : "NEFT");
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

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLIPayValidation + SignedReq);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            res.Response = IPayRespV;
            SaveDMTLog("FundTransferIPay", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);

            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    res.Request = (URLIPayPayment + SignedReq);
                    res.Response = IPayRespP;
                    SaveDMTLog("FundTransferIPay", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
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
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.BeneName = ADDINFO.data.benename;
                                    res.LiveID = ADDINFO.data.opr_id;
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public ResponseStatus BeneficiaryDeleteIPay(DMTReq _req, string BeniID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=6");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(_req.ReffID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benId=");
            reqBuilder.Append(BeniID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("BeneficiaryDeleteIPay", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("BeneficiaryDeleteIPay", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPay>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    if (ADDINFO.data.otp == 1)
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                        res.CommonBool = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public ResponseStatus BeneficiaryDeleteValidateIPay(DMTReq _req, string BeniID, string otp)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error,
                CommonBool = true
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, _req.TID);

            reqBuilder.Append("Type=23");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(_req.ReffID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benId=");
            reqBuilder.Append(BeniID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(otp);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("BeneficiaryDeleteIPay", _req.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, _req.SenderNO, _req.UserID, _req.TID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("BeneficiaryDeleteIPay", _req.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, _req.SenderNO, _req.UserID, _req.TID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPayData>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    if (ADDINFO.data == "Success")
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        res.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                                        res.CommonBool = false;
                                    }
                                }
                            }
                        }
                        else if (PResp.ERROR == "224" && PResp.RESULT == "1")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                            res.CommonBool = true;
                        }
                    }
                }
            }
            return res;
        }
    }

    public class CyberPlateMLIPay : IMoneyTransferAPIML
    {
        private readonly IDAL _dal;
        private const string SDCode = "";
        private const string APCode = "";
        private const string OPCode = "";
        private const string CyberPlatPWD = "";
        private const string CERTNO = "";

        private const string URLIPayValidation = "https://in.cyberplat.com/cgi-bin/instp/instp_pay_check.cgi";
        private const string URLIPayPayment = "https://in.cyberplat.com/cgi-bin/instp/instp_pay.cgi";
        private const string URLIPayStatus = "https://in.cyberplat.com/cgi-bin/instp/instp_pay_status.cgi";
        public CyberPlateMLIPay(IDAL dal) => _dal = dal;
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Sender_Not_Found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=5");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemitterDetailsIPay", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemitterDetailsIPay", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        PResp.ERROR = PResp.ERROR ?? "-1";
                        if (Convert.ToInt16(PResp.ERROR) == 0)
                        {
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if ((ADDINFO.statuscode ?? string.Empty) == "TXN")
                                {
                                    res.ReferenceID = ADDINFO.data.remitter.id;
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        res.RemainingLimit = ADDINFO.data.remitter.remaininglimit;
                                        res.AvailbleLimit = ADDINFO.data.remitter.consumedlimit + ADDINFO.data.remitter.remaininglimit;
                                        res.SenderMobile = request.SenderMobile;
                                        res.SenderName = ADDINFO.data.remitter.name;
                                        res.KYCStatus = SenderKYCStatus.ACTIVE;
                                    }
                                    else
                                    {
                                        res.IsOTPGenerated = true;
                                        res.KYCStatus = SenderKYCStatus.APPLIED;
                                    }
                                }
                            }
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
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("Type=4");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(request.ReferenceID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(request.mBeneDetail.BeneName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(request.mBeneDetail.BeneName);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(request.mBeneDetail.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(request.mBeneDetail.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("CreateBeneficiary", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("CreateBeneficiary", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPay>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                                    res.BeneID = ADDINFO.data.beneficiary.id;
                                    res.ReferenceID = ADDINFO.data.remitter.id;
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
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("Type=0");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("lName=");
            reqBuilder.Append(request.LastName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("fName=");
            reqBuilder.Append(request.FirstName ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Pin=");
            reqBuilder.Append(request.Pincode.ToString());
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemitterRegistrationIPay", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemitterRegistrationIPay", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                        res.ReferenceID = ADDINFO.data.remitter.id;
                                    }
                                    else
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                        res.ReferenceID = ADDINFO.data.remitter.id;
                                        res.IsOTPGenerated = true;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            return res;
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("Type=5");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("GetBeneficiary", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("GetBeneficiary", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        PResp.ERROR = PResp.ERROR ?? "-1";
                        if (Convert.ToInt16(PResp.ERROR) == 0)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if ((ADDINFO.statuscode ?? string.Empty) == "TXN")
                                {
                                    res.ReferenceID = ADDINFO.data.remitter.id;
                                    var Beneficiaries = new List<MBeneDetail>();
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        foreach (var item in ADDINFO.data.beneficiary)
                                        {
                                            Beneficiaries.Add(new MBeneDetail
                                            {
                                                AccountNo = item.account,
                                                BankName = item.bank,
                                                IFSC = item.ifsc,
                                                BeneName = item.name,
                                                MobileNo = item.mobile,
                                                BeneID = item.id
                                            });
                                        }
                                        res.Beneficiaries = Beneficiaries;
                                    }
                                    else
                                    {
                                        res.Statuscode = ErrorCodes.Minus1;
                                    }
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
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("Type=24");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(request.ReferenceID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(request.OTP);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("VerifySender", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("VerifySender", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {

                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfo>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    if (ADDINFO.data.remitter.is_verified == 1)
                                    {
                                        res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                                        res.ReferenceID = ADDINFO.data.remitter.id;
                                    }
                                    else
                                    {
                                        res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                        res.ErrorCode = ErrorCodes.Invalid_OTP;
                                    }
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
        private void SaveDMTLog(string Method, int RMode, string Req, string Res, string SenderNo, int UserID, string TID, int _APIID)
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

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if ((request.OTP ?? string.Empty).Length < 2)
            {
                #region RequestRegion
                StringBuilder reqBuilder = new StringBuilder();
                RequestHelper(reqBuilder, request.TransactionID);

                reqBuilder.Append("Type=6");
                reqBuilder.Append(Environment.NewLine);

                reqBuilder.Append("remId=");
                reqBuilder.Append(request.ReferenceID);
                reqBuilder.Append(Environment.NewLine);

                reqBuilder.Append("benId=");
                reqBuilder.Append(request.mBeneDetail.BeneID);
                reqBuilder.Append(Environment.NewLine);

                reqBuilder.Append("AMOUNT=1.00");
                reqBuilder.Append(Environment.NewLine);

                reqBuilder.Append("AMOUNT_ALL=1.00");
                reqBuilder.Append(Environment.NewLine);

                reqBuilder.Append("COMMENT=test");
                #endregion
                var openssl = new OpenSSL();
                var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
                var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
                SaveDMTLog("RemoveBeneficiary", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
                {
                    var VResp = SerializeCyberPlateResponse(IPayRespV);
                    if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                    {
                        var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                        SaveDMTLog("RemoveBeneficiary", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                        if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                        {
                            var PResp = SerializeCyberPlateResponse(IPayRespP);
                            if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                            {
                                string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                                if (Validate.O.ValidateJSON(Decoded))
                                {
                                    var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPay>(Decoded);
                                    if (ADDINFO.statuscode == "TXN")
                                    {
                                        if (ADDINFO.data.otp == 1)
                                        {
                                            res.Statuscode = ErrorCodes.One;
                                            res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                            res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                            res.IsOTPGenerated = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                res = RemoveBeneWithOTP(request);
            }
            return res;
        }
        private MSenderLoginResponse RemoveBeneWithOTP(MTAPIRequest request)
        {

            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error,
                IsOTPGenerated = true
            };
            #region RequestRegion
            StringBuilder reqBuilder = new StringBuilder();
            RequestHelper(reqBuilder, request.TransactionID);

            reqBuilder.Append("Type=23");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("remId=");
            reqBuilder.Append(request.ReferenceID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benId=");
            reqBuilder.Append(request.mBeneDetail.BeneID);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("otc=");
            reqBuilder.Append(request.OTP);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            SaveDMTLog("RemoveBeneficiary", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    SaveDMTLog("RemoveBeneficiary", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TransactionID, request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
                        if (Convert.ToInt16(PResp.ERROR ?? "-1") == 0)
                        {
                            string Decoded = HttpUtility.UrlDecode((PResp.ADDINFO ?? string.Empty));
                            if (Validate.O.ValidateJSON(Decoded))
                            {
                                var ADDINFO = JsonConvert.DeserializeObject<CyberAddInfoIPayData>(Decoded);
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    if (ADDINFO.data == "Success")
                                    {
                                        res.Statuscode = ErrorCodes.One;
                                        res.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted).Replace("_", " ");
                                        res.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                                        res.IsOTPGenerated = false;
                                    }
                                }
                            }
                        }
                        else if (PResp.ERROR == "224" && PResp.RESULT == "1")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                    }
                }
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

            reqBuilder.Append("Type=10");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benAccount=");
            reqBuilder.Append(request.mBeneDetail.AccountNo ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("benIFSC=");
            reqBuilder.Append(request.mBeneDetail.IFSC ?? string.Empty);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("AMOUNT_ALL=1.00");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLIPayValidation + SignedReq);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            res.Response = IPayRespV;
            SaveDMTLog("VerifyAccount", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TID.ToString(),request.APIID);

            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    res.Request = (URLIPayPayment + SignedReq);
                    res.Response = IPayRespP;
                    SaveDMTLog("VerifyAccount", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TID.ToString(),request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
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
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.BeneName = ADDINFO.data.benename;
                                    res.LiveID = ADDINFO.data.bankrefno;
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

            reqBuilder.Append("Type=3");
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("NUMBER=");
            reqBuilder.Append(request.SenderMobile);
            reqBuilder.Append(Environment.NewLine);

            reqBuilder.Append("routingType=");
            reqBuilder.Append(request.TransMode);
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

            reqBuilder.Append("COMMENT=test");
            #endregion
            var openssl = new OpenSSL();
            var SignedReq = openssl.Sign_With_PFX(reqBuilder.ToString(), DOCType.CyberPlatFilePath, CyberPlatPWD);
            res.Request = (URLIPayValidation + SignedReq);
            var IPayRespV = openssl.CallCryptoAPI(SignedReq, URLIPayValidation);
            res.Response = IPayRespV;
            SaveDMTLog("AccountTransfer", request.RequestMode, (URLIPayValidation + SignedReq), IPayRespV, request.SenderMobile, request.UserID, request.TID.ToString(), request.APIID);

            if (!string.IsNullOrEmpty(IPayRespV) && !(IPayRespV ?? string.Empty).Contains(OpenSSL.EXKEY))
            {
                var VResp = SerializeCyberPlateResponse(IPayRespV);
                if (Convert.ToInt16(VResp.ERROR ?? "-1") == 0)
                {
                    res.VendorID = VResp.TRANSID;
                    var IPayRespP = openssl.CallCryptoAPI(SignedReq, URLIPayPayment);
                    res.Request = (URLIPayPayment + SignedReq);
                    res.Response = IPayRespP;
                    SaveDMTLog("AccountTransfer", request.RequestMode, (URLIPayPayment + SignedReq), IPayRespP, request.SenderMobile, request.UserID, request.TID.ToString(), request.APIID);
                    if (!string.IsNullOrEmpty(IPayRespP) && !(IPayRespP ?? string.Empty).Contains(OpenSSL.EXKEY))
                    {
                        var PResp = SerializeCyberPlateResponse(IPayRespP);
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
                                if (ADDINFO.statuscode == "TXN")
                                {
                                    res.BeneName = ADDINFO.data.benename;
                                    res.LiveID = ADDINFO.data.opr_id;
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
