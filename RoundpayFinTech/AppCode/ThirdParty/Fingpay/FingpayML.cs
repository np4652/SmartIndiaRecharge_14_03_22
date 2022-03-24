using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.Fingpay
{
    public partial class FingpayML
    {
        private const string BalanceCheckURL = "https://fingpayap.tapits.in/fpaepsservice/api/balanceInquiry/merchant/getBalance";
        private const string WithDrawlURL = "https://fingpayap.tapits.in/fpaepsservice/api/cashWithdrawal/merchant/withdrawal";
        private const string AdharPayURL = "https://fingpayap.tapits.in/fpaepsservice/api/aadhaarPay/merchant/pay";
        private const string OnBoardingURL = "https://fingpayap.tapits.in/fpaepsweb/api/onboarding/merchant/creation/m1";
        private const string StatusCheckURL = "https://fpma.tapits.in/fpcardwebservice/api/ma/statuscheck/cw";
        private const string AEPSStatusCheckURL = "https://fingpayap.tapits.in/fpaepsweb/api/auth/merchantInfo/statusCheckV2/merchantLoginId/cashWithdrawal/v2";
        private const string StatementURL = "https://fingpayap.tapits.in/fpaepsservice/api/miniStatement/merchant/statement";
        private const string DepositURLGenerateOTP = "https://fingpayap.tapits.in/fpaepsservice/api/CashDeposit/merchant/generate/otp";
        private const string DepositURLGenerateOTP_UAT = "https://fpuat.tapits.in/fpaepsservice/api/CashDeposit/merchant/generate/otp";
        private const string DepositURLValidateOTP = "https://fingpayap.tapits.in/fpaepsservice/api/CashDeposit/merchant/validate/otp";
        private const string DepositURLTransaction = "https://fingpayap.tapits.in/fpaepsservice/api/CashDeposit/merchant/transaction";

        private readonly IDAL _dal;
        public FingpayML(IDAL dal) => _dal = dal;
        //public Fingpay
        public OutletAPIStatusUpdate TwoFactorAuthentication(ValidateAPIOutletResp _ValidateAPIOutletResp, FingpayAPISetting FPSetting)
        {

            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var Req = string.Empty;
            var Resp = string.Empty;
            var pidData = _ValidateAPIOutletResp.pidData;
            var Lattitude = (_ValidateAPIOutletResp.Latlong ?? string.Empty).Contains(",") ? _ValidateAPIOutletResp.Latlong.Split(',')[0] : (_ValidateAPIOutletResp.Latlong ?? string.Empty);
            var Longitude = (_ValidateAPIOutletResp.Latlong ?? string.Empty).Contains(",") ? _ValidateAPIOutletResp.Latlong.Split(',')[1] : (_ValidateAPIOutletResp.Latlong ?? string.Empty);
            try
            {
                string _URL = "https://fingpayap.tapits.in/fpaepsservice/auth/tfauth/merchant/validate/aadhar";
                string nationalBankIdentificationNumber = "607152";
                var req = new
                {
                    superMerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                    //merchantLoginId = "FP" + _ValidateAPIOutletResp.OutletID,
                    //primaryKeyId = _ValidateAPIOutletResp.APIReferenceID,
                    //encodeFPTxnId = _ValidateAPIOutletResp.APIHash,
                    requestRemarks = "For Twoway authentication " + (_ValidateAPIOutletResp.Name ?? string.Empty),
                    transactionType = "AUO",
                    merchantUserName = "FP" + _ValidateAPIOutletResp.OutletID,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    mobileNumber = _ValidateAPIOutletResp.MobileNo,
                    latitude = Lattitude,
                    longitude = Longitude,
                    merchantTranId = _ValidateAPIOutletResp.TransactionID,
                    cardnumberORUID = new
                    {
                        nationalBankIdentificationNumber = nationalBankIdentificationNumber,
                        indicatorforUID = "0",
                        adhaarNumber = _ValidateAPIOutletResp.AADHAR
                    },
                    captureResponse = new CaptureResponse
                    {
                        errCode = pidData.Resp.ErrCode,
                        errInfo = pidData.Resp.ErrInfo,
                        fCount = pidData.Resp.FCount,
                        fType = pidData.Resp.FType,
                        iCount = "0",
                        pCount = "0",
                        pType = "0",
                        nmPoints = pidData.Resp.NmPoints,
                        qScore = pidData.Resp.QScore,
                        dpID = pidData.DeviceInfo.DpId,
                        rdsID = pidData.DeviceInfo.RdsId,
                        rdsVer = pidData.DeviceInfo.RdsVer,
                        dc = pidData.DeviceInfo.Dc,
                        mi = pidData.DeviceInfo.Mi,
                        mc = pidData.DeviceInfo.Mc,
                        ci = pidData.Skey.Ci,
                        sessionKey = pidData.Skey.Text,
                        hmac = pidData.Hmac,
                        PidDatatype = pidData.Data.Type,
                        Piddata = pidData.Data.Text
                    }
                };
                var TIMESTAMP = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = _ValidateAPIOutletResp.TransactionID;
                try
                {
                    if (pidData.DeviceInfo.additionalInfo.Param.Count > 0)
                    {
                        deviceIMEI = pidData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                    }
                }
                catch (Exception)
                {
                }
                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp",TIMESTAMP.Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",deviceIMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                Req = _URL + "/" + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                Resp = AppWebRequest.O.HWRPost(_URL, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(Resp))
                {
                    var apiResp = JsonConvert.DeserializeObject<FingpayTwowayResponse>(Resp);
                    if (apiResp != null)
                    {
                        if (apiResp.status)
                        {
                            _resp.Statuscode = ErrorCodes.Minus1;
                            _resp.Msg = apiResp.message;
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.responseCode == "00")
                                {
                                    _resp.Statuscode = ErrorCodes.One;
                                    _resp.Msg = "Twoway authentication done";
                                }
                            }

                        }
                        else
                        {
                            _resp.Statuscode = ErrorCodes.Minus1;
                            _resp.Msg = apiResp.message;
                            if (string.IsNullOrEmpty(apiResp.message))
                            {
                                _resp.Msg = apiResp.data != null ? apiResp.data.responseMessage : nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Resp = "Exception[" + ex.Message + "]" + Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "TwoFactorAuthentication",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _ValidateAPIOutletResp.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _ValidateAPIOutletResp.APIID,
                Method = "FingpayML.TwoFactorAuthentication",
                Request = Req,
                Response = Resp

            });
            #endregion
            return _resp;
        }
        public BalanceEquiryResp GetBalance(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, PidData pidData, string Aadhar, string Bank, string merchantTransactionId, string subMerchantId, string IMEI, string Lattitude, string Longitude)
        {
            var res = new BalanceEquiryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error)
            };

            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var balanceEnquirey = new BalnaceEquiry
                {
                    languageCode = "en",
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    transactionType = "BE",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = fingpayAPISetting.superMerchantId,
                    //subMerchantId = subMerchantId,
                    merchantTransactionId = merchantTransactionId,
                    captureResponse = new CaptureResponse
                    {
                        PidDatatype = pidData.Data.Type,
                        Piddata = pidData.Data.Text,
                        ci = pidData.Skey.Ci,
                        dc = pidData.DeviceInfo.Dc,
                        dpID = pidData.DeviceInfo.DpId,
                        errCode = pidData.Resp.ErrCode,
                        errInfo = pidData.Resp.ErrInfo,
                        fCount = pidData.Resp.FCount,
                        fType = pidData.Resp.FType,
                        hmac = pidData.Hmac,
                        iCount = "0",
                        mc = pidData.DeviceInfo.Mc,
                        mi = pidData.DeviceInfo.Mi,
                        nmPoints = pidData.Resp.NmPoints,
                        pCount = "0",
                        pType = "0",
                        qScore = pidData.Resp.QScore,
                        rdsID = pidData.DeviceInfo.RdsId,
                        rdsVer = pidData.DeviceInfo.RdsVer,
                        sessionKey = pidData.Skey.Text
                    },
                    cardnumberORUID = new CardnumberORUID
                    {
                        adhaarNumber = Aadhar ?? string.Empty,
                        indicatorforUID = 0,
                        nationalBankIdentificationNumber = Bank ?? string.Empty
                    }
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(balanceEnquirey));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = string.IsNullOrEmpty(IMEI) ? "IMI" + merchantTransactionId : IMEI;
                try
                {
                    if (pidData.DeviceInfo.additionalInfo.Param.Count > 0)
                    {
                        deviceIMEI = pidData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                    }
                }
                catch (Exception)
                {
                }
                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", balanceEnquirey.timestamp.Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",deviceIMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = BalanceCheckURL + "/" + JsonConvert.SerializeObject(balanceEnquirey) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(BalanceCheckURL, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<DeviceResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;
                        if (deviceResp.status)
                        {
                            res.Statuscode = deviceResp.data.transactionStatus == "successful" ? ErrorCodes.One : ErrorCodes.Minus1;
                            res.Balance = deviceResp.data.balanceAmount;
                            res.BankRRN = deviceResp.data.bankRRN;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBalance",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }
        public WithdrawlResponse Withdraw(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, PidData pidData, string Aadhar, string Bank, int Amount, string merchantTransactionId, string subMerchantId, string IMEI, string Lattitude, string Longitude)
        {
            var res = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING,
                Errorcode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var withDrawEquiry = new WithDrawEquiry
                {
                    languageCode = "en",
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    transactionType = "CW",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = fingpayAPISetting.superMerchantId,
                    //subMerchantId = subMerchantId,
                    merchantTranId = merchantTransactionId,
                    transactionAmount = Amount.ToString(),
                    captureResponse = new CaptureResponse
                    {
                        PidDatatype = pidData.Data.Type,
                        Piddata = pidData.Data.Text,
                        ci = pidData.Skey.Ci,
                        dc = pidData.DeviceInfo.Dc,
                        dpID = pidData.DeviceInfo.DpId,
                        errCode = pidData.Resp.ErrCode,
                        errInfo = pidData.Resp.ErrInfo,
                        fCount = pidData.Resp.FCount,
                        fType = pidData.Resp.FType,
                        hmac = pidData.Hmac,
                        iCount = "0",
                        mc = pidData.DeviceInfo.Mc,
                        mi = pidData.DeviceInfo.Mi,
                        nmPoints = pidData.Resp.NmPoints,
                        pCount = "0",
                        pType = "0",
                        qScore = pidData.Resp.QScore,
                        rdsID = pidData.DeviceInfo.RdsId,
                        rdsVer = pidData.DeviceInfo.RdsVer,
                        sessionKey = pidData.Skey.Text
                    },
                    cardnumberORUID = new CardnumberORUID
                    {
                        adhaarNumber = Aadhar ?? string.Empty,
                        indicatorforUID = 0,
                        nationalBankIdentificationNumber = Bank ?? string.Empty
                    }
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(withDrawEquiry));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = string.IsNullOrEmpty(IMEI) ? "IMI" + merchantTransactionId : IMEI;
                try
                {
                    if (pidData.DeviceInfo.additionalInfo.Param.Count > 0)
                    {

                        deviceIMEI = pidData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;

                    }
                }
                catch (Exception)
                {
                }
                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", withDrawEquiry.timestamp.Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",deviceIMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = WithDrawlURL + "/" + JsonConvert.SerializeObject(withDrawEquiry) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(WithDrawlURL, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<DeviceResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;

                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatus.ToLower().Equals("successful"))
                                {
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.Status = RechargeRespType.SUCCESS;
                                    res.Errorcode = ErrorCodes.Transaction_Successful;
                                }
                                res.Balance = deviceResp.data.balanceAmount;
                                res.LiveID = deviceResp.data.bankRRN;
                                res.VendorID = deviceResp.data.fpTransactionId;
                                if (deviceResp.data.transactionStatus.ToLower().Equals("failed"))
                                {
                                    res.LiveID = res.Msg;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatusCode == "FP009")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted);
                                    res.Status = RechargeRespType.PENDING;
                                    res.Errorcode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = deviceResp.data.bankRRN;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Withdraw",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public DepositResponse GenerateDepositOTP(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, string AccountNo, string Bank, int Amount, string merchantTransactionId, string subMerchantId, string IMEI, string Lattitude, string Longitude)
        {
            var res = new DepositResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                Errorcode = ErrorCodes.Unknown_Error
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var FPRequest = new FPDepositRequest
                {
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    transactionType = "CDO",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = Convert.ToInt32(fingpayAPISetting.superMerchantId),
                    //subMerchantId = subMerchantId,
                    merchantTranId = merchantTransactionId,
                    amount = Amount,
                    accountNumber = AccountNo,
                    fingpayTransactionId = string.Empty,
                    otp = string.Empty,
                    cdPkId = 0,
                    iin = Bank,
                    secretKey = fingpayAPISetting.secretKey
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest) + fingpayAPISetting.secretKey));
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);

                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",string.IsNullOrEmpty(IMEI)? ("IMI" + merchantTransactionId) : IMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = DepositURLGenerateOTP + "/" + JsonConvert.SerializeObject(FPRequest) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(DepositURLGenerateOTP, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<FPDepositResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;

                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.responseCode == "00")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                    res.Status = RechargeRespType.SUCCESS;
                                    res.Errorcode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                    res.LiveID = deviceResp.data.bankRrn;
                                    res.VendorID = deviceResp.data.fingpayTransactionId;
                                    res.RefferenceNo = deviceResp.data.cdPkId.ToString();
                                }
                                else
                                {
                                    res.Msg = deviceResp.data.responseMessage;
                                    res.Status = RechargeRespType.FAILED;
                                    res.Errorcode = DMTErrorCodes.Transaction_Failed;
                                }
                            }
                        }
                        else
                        {
                            res.Msg = deviceResp.message;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateDepositOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public DepositResponse ValidateDepositOTP(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, string AccountNo, string Bank, int Amount, string merchantTransactionId, string subMerchantId, string OTP, string RefferenceID, string VendorID, string IMEI, string Lattitude, string Longitude)
        {
            var res = new DepositResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED,
                Errorcode = ErrorCodes.Unknown_Error
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var FPRequest = new FPDepositRequest
                {
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    transactionType = "CDO",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = Convert.ToInt32(fingpayAPISetting.superMerchantId),
                    //subMerchantId = subMerchantId,
                    merchantTranId = merchantTransactionId,
                    amount = Amount,
                    accountNumber = AccountNo,
                    fingpayTransactionId = VendorID,
                    otp = OTP,
                    cdPkId = Convert.ToInt32(RefferenceID),
                    iin = Bank,
                    secretKey = fingpayAPISetting.secretKey
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest) + fingpayAPISetting.secretKey));
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);

                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",string.IsNullOrEmpty(IMEI)? ("IMI" + merchantTransactionId) :IMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = DepositURLValidateOTP + "/" + JsonConvert.SerializeObject(FPRequest) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(DepositURLValidateOTP, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<FPDepositResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;
                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.responseCode == "00")
                                {
                                    res.Msg = nameof(DMTErrorCodes.OTP_verified_successfully).Replace("_", " ");
                                    res.Statuscode = ErrorCodes.One;
                                    res.Errorcode = DMTErrorCodes.OTP_verified_successfully;
                                    res.LiveID = deviceResp.data.bankRrn;
                                    res.VendorID = deviceResp.data.fingpayTransactionId;
                                    res.BeneficaryName = deviceResp.data.beneficiaryName;
                                }
                                else
                                {
                                    res.Statuscode = ErrorCodes.Minus1;
                                    res.Msg = deviceResp.data.responseMessage;
                                    res.Status = RechargeRespType.FAILED;
                                    res.Errorcode = DMTErrorCodes.Transaction_Failed;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = deviceResp.message;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateDepositOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public DepositResponse DepositTransaction(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, string AccountNo, string Bank, int Amount, string merchantTransactionId, string subMerchantId, string OTP, string RefferenceID, string VendorID, string IMEI, string Lattitude, string Longitude)
        {
            var res = new DepositResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING,
                Errorcode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var FPRequest = new FPDepositRequest
                {
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    transactionType = "CDO",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = Convert.ToInt32(fingpayAPISetting.superMerchantId),
                    //subMerchantId = subMerchantId,
                    merchantTranId = merchantTransactionId,
                    amount = Amount,
                    accountNumber = AccountNo,
                    fingpayTransactionId = VendorID,
                    otp = OTP,
                    cdPkId = Convert.ToInt32(RefferenceID),
                    iin = Bank,
                    secretKey = fingpayAPISetting.secretKey
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(FPRequest) + fingpayAPISetting.secretKey));
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);

                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",string.IsNullOrEmpty(IMEI)? ("IMI" + merchantTransactionId) :IMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = DepositURLTransaction + "/" + JsonConvert.SerializeObject(FPRequest) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(DepositURLTransaction, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<FPDepositResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;
                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                res.LiveID = deviceResp.data.bankRrn;
                                res.VendorID = deviceResp.data.fingpayTransactionId;
                                if (deviceResp.data.responseCode == "00")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.Status = RechargeRespType.SUCCESS;
                                    res.Errorcode = ErrorCodes.Transaction_Successful;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = deviceResp.message;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateDepositOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public WithdrawlResponse AadharPay(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, PidData pidData, string Aadhar, string Bank, int Amount, string merchantTransactionId, string subMerchantId, string IMEI, string Lattitude, string Longitude)
        {
            var res = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING,
                Errorcode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var withDrawEquiry = new WithDrawEquiry
                {
                    languageCode = "en",
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    transactionType = "M",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    superMerchantId = fingpayAPISetting.superMerchantId,
                    //subMerchantId = subMerchantId,
                    merchantTranId = merchantTransactionId,
                    transactionAmount = Amount.ToString(),
                    captureResponse = new CaptureResponse
                    {
                        PidDatatype = pidData.Data.Type,
                        Piddata = pidData.Data.Text,
                        ci = pidData.Skey.Ci,
                        dc = pidData.DeviceInfo.Dc,
                        dpID = pidData.DeviceInfo.DpId,
                        errCode = pidData.Resp.ErrCode,
                        errInfo = pidData.Resp.ErrInfo,
                        fCount = pidData.Resp.FCount,
                        fType = pidData.Resp.FType,
                        hmac = pidData.Hmac,
                        iCount = "0",
                        mc = pidData.DeviceInfo.Mc,
                        mi = pidData.DeviceInfo.Mi,
                        nmPoints = pidData.Resp.NmPoints,
                        pCount = "0",
                        pType = "0",
                        qScore = pidData.Resp.QScore,
                        rdsID = pidData.DeviceInfo.RdsId,
                        rdsVer = pidData.DeviceInfo.RdsVer,
                        sessionKey = pidData.Skey.Text
                    },
                    cardnumberORUID = new CardnumberORUID
                    {
                        adhaarNumber = Aadhar ?? string.Empty,
                        indicatorforUID = 0,
                        nationalBankIdentificationNumber = Bank ?? string.Empty
                    }
                };
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(withDrawEquiry));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = string.IsNullOrEmpty(IMEI) ? ("IMI" + merchantTransactionId) : IMEI;

                try
                {
                    if (pidData.DeviceInfo.additionalInfo.Param.Count > 0)
                    {
                        deviceIMEI = pidData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                    }
                }
                catch (Exception)
                {
                }

                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", withDrawEquiry.timestamp.Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",deviceIMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = AdharPayURL + "/" + JsonConvert.SerializeObject(withDrawEquiry) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(AdharPayURL, EncryptUsingSessionKey, headers);
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<DeviceResponse>(res.Resp);
                    if (deviceResp != null)
                    {
                        res.Msg = deviceResp.message;

                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatus.ToLower().Equals("successful"))
                                {
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.Status = RechargeRespType.SUCCESS;
                                    res.Errorcode = ErrorCodes.Transaction_Successful;
                                }
                                res.Balance = deviceResp.data.balanceAmount;
                                res.LiveID = deviceResp.data.bankRRN;
                                res.VendorID = deviceResp.data.fpTransactionId;
                                if (deviceResp.data.transactionStatus.ToLower().Equals("failed"))
                                {
                                    res.LiveID = res.Msg;
                                }
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatusCode == "FP009")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted);
                                    res.Status = RechargeRespType.PENDING;
                                    res.Errorcode = ErrorCodes.Request_Accpeted;
                                    res.LiveID = deviceResp.data.bankRRN;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Withdraw",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public MiniStatementResponse MiniStatement(FingpayAPISetting fingpayAPISetting, UserDataForAEPS userDataForAEPS, PidData pidData, string Aadhar, string BankIIN, string merchantTransactionId, string subMerchantId, string IMEI, string Lattitude, string Longitude)
        {
            var res = new MiniStatementResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING
            };
            try
            {
                var lati = string.IsNullOrEmpty(Lattitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[0] : "0.0") : Lattitude;
                var longi = string.IsNullOrEmpty(Longitude) ? ((userDataForAEPS.LatLong ?? string.Empty).Contains(",") ? userDataForAEPS.LatLong.Split(',')[1] : "0.0") : Longitude;
                var STMTReq = new
                {
                    merchantTranId = merchantTransactionId,
                    captureResponse = new CaptureResponse
                    {
                        PidDatatype = pidData.Data.Type,
                        Piddata = pidData.Data.Text,
                        ci = pidData.Skey.Ci,
                        dc = pidData.DeviceInfo.Dc,
                        dpID = pidData.DeviceInfo.DpId,
                        errCode = pidData.Resp.ErrCode,
                        errInfo = pidData.Resp.ErrInfo,
                        fCount = pidData.Resp.FCount,
                        fType = pidData.Resp.FType,
                        hmac = pidData.Hmac,
                        iCount = "0",
                        mc = pidData.DeviceInfo.Mc,
                        mi = pidData.DeviceInfo.Mi,
                        nmPoints = pidData.Resp.NmPoints,
                        pCount = "0",
                        pType = "0",
                        qScore = pidData.Resp.QScore,
                        rdsID = pidData.DeviceInfo.RdsId,
                        rdsVer = pidData.DeviceInfo.RdsVer,
                        sessionKey = pidData.Skey.Text
                    },
                    cardnumberORUID = new CardnumberORUID
                    {
                        adhaarNumber = Aadhar ?? string.Empty,
                        indicatorforUID = 0,
                        nationalBankIdentificationNumber = BankIIN ?? string.Empty
                    },
                    languageCode = "en",
                    latitude = lati.Contains(".") ? Convert.ToDouble(lati.Trim()) : 0.0,
                    longitude = longi.Contains(".") ? Convert.ToDouble(longi.Trim()) : 0.0,
                    mobileNumber = userDataForAEPS.MobileNo,
                    paymentType = "B",
                    requestRemarks = "TN3000CA0006530",
                    timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    transactionType = "MS",
                    merchantUserName = subMerchantId,//fingpayAPISetting.MERCHANTName,
                    merchantPin = HashEncryption.O.MD5Hash("1234").ToLower(),
                    fingpayAPISetting.superMerchantId
                };

                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(STMTReq));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = string.IsNullOrEmpty(IMEI) ? ("IMI" + merchantTransactionId) : IMEI;
                try
                {
                    if (pidData.DeviceInfo.additionalInfo.Param.Count > 0)
                    {

                        deviceIMEI = pidData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                    }
                }
                catch (Exception)
                {
                }
                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", STMTReq.timestamp.Replace("-", "/") },
                    { "Hash", Hash },
                    { "DeviceIMEI",deviceIMEI},
                    { "Eskey", EncryptUsintPublicKey}
                };
                res.Req = StatementURL + "/" + JsonConvert.SerializeObject(STMTReq) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                res.Resp = AppWebRequest.O.HWRPost(StatementURL, EncryptUsingSessionKey, headers);
                res.Statements = new List<MiniStatement>();
                if (!string.IsNullOrEmpty(res.Resp))
                {
                    var deviceResp = JsonConvert.DeserializeObject<DeviceResponse>(res.Resp);
                    if (deviceResp != null)
                    {

                        res.Msg = (deviceResp.message ?? string.Empty).ToLower().Contains("insuf") ? "Provider down" : deviceResp.message;
                        if (deviceResp.status)
                        {
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatus.ToLower().Equals("successful"))
                                {
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.Status = RechargeRespType.SUCCESS;
                                    res.Balance = deviceResp.data.balanceAmount;
                                }
                                res.LiveID = deviceResp.data.bankRRN;
                                res.VendorID = deviceResp.data.fpTransactionId;

                                if (deviceResp.data.transactionStatus.ToLower().Equals("failed"))
                                {
                                    res.LiveID = res.Msg;
                                    res.Status = RechargeRespType.FAILED;
                                }
                                if (deviceResp.data.miniStatementStructureModel != null)
                                {
                                    foreach (var item in deviceResp.data.miniStatementStructureModel)
                                    {
                                        res.Statements.Add(new MiniStatement
                                        {
                                            TransactionDate = item.date,
                                            TransactionType = item.txnType,
                                            Amount = item.amount,
                                            Narration = item.narration
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            res.LiveID = res.Msg;
                            res.Status = RechargeRespType.FAILED;
                            if (deviceResp.data != null)
                            {
                                if (deviceResp.data.transactionStatusCode == "FP009")
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = nameof(ErrorCodes.Request_Accpeted);
                                    res.Status = RechargeRespType.PENDING;
                                    res.LiveID = deviceResp.data.bankRRN;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Resp = "Exception[" + ex.Message + "]" + res.Resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MiniStatement",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = userDataForAEPS.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public OutletAPIStatusUpdate MerchantOnboarding(FingpayReq fingpayReq, int APIID)
        {
            string Req = string.Empty;
            string Resp = string.Empty;
            var OutReqRes = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            try
            {
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fingpayReq));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);

                var headers = new Dictionary<string, string>
                {
                    { "trnTimestamp", fingpayReq.timestamp },
                    { "Hash", Hash },
                    { "Eskey", EncryptUsintPublicKey}
                };
                Req = OnBoardingURL + "/" + JsonConvert.SerializeObject(fingpayReq) + JsonConvert.SerializeObject(headers) + "?" + EncryptUsingSessionKey;
                Resp = AppWebRequest.O.HWRPost(OnBoardingURL, EncryptUsingSessionKey, headers);

                if (Validate.O.ValidateJSON(Resp ?? string.Empty))
                {
                    //Resp = JsonConvert.DeserializeObject<string>(Resp);
                    var _apiRes = JsonConvert.DeserializeObject<FingpayResponse>(Resp);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status && _apiRes.statusCode == 0)
                        {
                            OutReqRes.Statuscode = ErrorCodes.One;
                            OutReqRes.Msg = ErrorCodes.OutletRegistered;
                            OutReqRes.APIOutletID = _apiRes.data.merchants[0].merchantLoginId;
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.KYCStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.BBPSID = _apiRes.data.merchants[0].merchantLoginId;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.AEPSID = _apiRes.data.merchants[0].merchantLoginId;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Approved;
                            OutReqRes.DMTID = _apiRes.data.merchants[0].merchantLoginId;
                            OutReqRes.DMTStatus = RndKYCOutLetStatus._NotApplied;
                            OutReqRes.PSAID = string.Empty;
                            OutReqRes.PSAStatus = RndKYCOutLetStatus._NotApplied;
                        }
                        else
                        {
                            OutReqRes.APIOutletStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.BBPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.AEPSStatus = RndKYCOutLetStatus._Rejected;
                            OutReqRes.Msg = string.IsNullOrEmpty(_apiRes.message) ? ErrorCodes.FailedToSubmit : _apiRes.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MerchantOnboarding",
                    Error = ex.Message,
                });
                Resp = ex.Message + "_" + Resp;
                OutReqRes.Msg = ErrorCodes.TempError;
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = APIID,
                Method = "MerchantOnboarding",
                Request = Req,
                Response = Resp,

            });
            #endregion
            return OutReqRes;
        }

        public MiniBankTransactionServiceResp MiniBankStatusCheck(FingpayAPISetting fingpayAPISetting, MBStatusCheckRequest request)
        {
            var response = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            var fingpayReq = new FPMiniBankStatusCheckRequest
            {
                merchantTranId = request.TID.ToString(),
                merchantLoginId = "FP" + request.OutletID,
                merchantPassword = HashEncryption.O.MD5Hash("1234"),
                superMerchantId = Convert.ToInt32(fingpayAPISetting.superMerchantId),
                superMerchantPassword = fingpayAPISetting.MerchantPin
            };
            string MGResp = string.Empty;
            try
            {
                var Sbytes = Encoding.UTF8.GetBytes(string.Concat(fingpayReq.merchantTranId, fingpayReq.merchantLoginId, fingpayReq.superMerchantId).ToLower());
                fingpayReq.hash = HashEncryption.O.GenerateSha256Hash(Sbytes);
                if (!ApplicationSetting.IsHostedServerIsIndian)
                {
                    MGResp = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = StatusCheckURL, obj = fingpayReq });
                }
                else
                {
                    MGResp = AppWebRequest.O.PostJsonDataUsingHWR(StatusCheckURL, fingpayReq);
                }
                try
                {
                    MGResp = JsonConvert.DeserializeObject<string>(MGResp);
                }
                catch (Exception)
                {
                }

                var _apiResp = JsonConvert.DeserializeObject<FPMiniBankStatusCheckResponse>(MGResp);
                if (_apiResp != null)
                {
                    if (_apiResp.status)
                    {
                        response.Msg = _apiResp.message ?? string.Empty;
                        if (_apiResp.data != null && _apiResp.data.Count > 0)
                        {
                            response.Msg = _apiResp.data[0].transactionStatusMessage;
                            response.VendorID = _apiResp.data[0].fingpayTransactionId;
                            response.Amount = Convert.ToInt32(_apiResp.data[0].transactionAmount);
                            response.LiveID = _apiResp.data[0].bankRRN;
                            response.CardNumber = _apiResp.data[0].cardNumber;
                            response.BankBalance = _apiResp.data[0].balanceAmount.ToString();
                            response.BankTransactionDate = _apiResp.data[0].transactionTime;
                            response.BankName = _apiResp.data[0].bankName;
                            if (_apiResp.data[0].transactionStatus && _apiResp.data[0].transactionStatusMessage.Equals("Success"))
                            {
                                response.Statuscode = RechargeRespType.SUCCESS;
                            }
                            else if (_apiResp.data[0].transactionStatus == false)
                            {
                                response.Statuscode = RechargeRespType.FAILED;
                                response.LiveID = _apiResp.data[0].transactionStatusMessage;
                            }
                            else
                            {
                                response.Statuscode = RechargeRespType.PENDING;
                            }
                            if (_apiResp.data[0].transactionStatusCode.In("FP009", "FP000"))
                            {
                                response.Msg = _apiResp.data[0].transactionStatusMessage;
                                response.VendorID = _apiResp.data[0].fingpayTransactionId;
                                response.Amount = Convert.ToInt32(_apiResp.data[0].transactionAmount);
                                response.LiveID = _apiResp.data[0].bankRRN;
                                response.CardNumber = _apiResp.data[0].cardNumber;
                                response.BankBalance = _apiResp.data[0].balanceAmount.ToString();
                                response.BankTransactionDate = _apiResp.data[0].transactionTime;
                                response.BankName = _apiResp.data[0].bankName;
                                response.Statuscode = RechargeRespType.PENDING;
                            }
                        }
                    }
                    else
                    {
                        if (_apiResp.statusCode == 0)
                        {
                            response.Statuscode = RechargeRespType.FAILED;
                            response.LiveID = ErrorCodes.CanceledByUser;
                            response.Msg = _apiResp.message;
                        }
                        if (_apiResp.data != null)
                        {
                            if (_apiResp.data[0].transactionStatusCode.In("FP009", "FP000"))
                            {
                                response.Msg = _apiResp.data[0].transactionStatusMessage;
                                response.VendorID = _apiResp.data[0].fingpayTransactionId;
                                response.Amount = Convert.ToInt32(_apiResp.data[0].transactionAmount);
                                response.LiveID = _apiResp.data[0].bankRRN;
                                response.CardNumber = _apiResp.data[0].cardNumber;
                                response.BankBalance = _apiResp.data[0].balanceAmount.ToString();
                                response.BankTransactionDate = _apiResp.data[0].transactionTime;
                                response.BankName = _apiResp.data[0].bankName;
                                response.Statuscode = RechargeRespType.PENDING;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MiniBankStatusCheck",
                    Error = ex.Message,
                });
                response.Msg = ErrorCodes.TempError;
                MGResp = ex.Message + "_" + MGResp;
            }
            response.Req = StatusCheckURL + "?" + JsonConvert.SerializeObject(fingpayReq);
            response.Resp = MGResp;
            return response;
        }

        public MiniBankTransactionServiceResp AEPSStatusCheck(FingpayAPISetting fingpayAPISetting, MBStatusCheckRequest request)
        {
            var response = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };
            var fingpayReq = new FPMiniBankStatusCheckRequest
            {
                merchantTranId = request.TransactionID,
                merchantLoginId = ("FP" + request.OutletID),
                merchantPassword = HashEncryption.O.MD5Hash("1234").ToLower(),
                superMerchantId = Convert.ToInt32(fingpayAPISetting.superMerchantId),
                superMerchantPassword = fingpayAPISetting.MerchantPin
            };


            string MGResp = string.Empty;
            try
            {
                var Sbytes = Encoding.UTF8.GetBytes(string.Concat(fingpayReq.merchantTranId, "+", fingpayReq.merchantLoginId.ToLower(), "+", fingpayAPISetting.MERCHANTName));
                fingpayReq.hash = HashEncryption.O.GenerateSha256Hash(Sbytes);

                MGResp = AppWebRequest.O.PostJsonDataUsingHWR(AEPSStatusCheckURL, fingpayReq);
                var _apiResp = JsonConvert.DeserializeObject<FPMiniBankStatusCheckResponse>(MGResp);
                if (_apiResp != null)
                {
                    if (_apiResp.apiStatus)
                    {
                        response.Msg = _apiResp.apiStatusMessage ?? string.Empty;
                        if (_apiResp.data.Count > 0)
                        {
                            response.Msg = _apiResp.data[0].transactionStatusMessage;
                            response.VendorID = _apiResp.data[0].fingpayTransactionId;
                            response.Amount = Convert.ToInt32(_apiResp.data[0].transactionAmount);
                            response.LiveID = _apiResp.data[0].bankRRN;
                            response.CardNumber = _apiResp.data[0].aadhaarNumber;
                            response.BankBalance = _apiResp.data[0].balanceAmount.ToString();
                            response.BankTransactionDate = _apiResp.data[0].transactionTime;
                            response.BankName = _apiResp.data[0].bankName;
                            if (_apiResp.data[0].transactionStatus && _apiResp.data[0].transactionStatusMessage.Equals("Success"))
                            {
                                response.Statuscode = RechargeRespType.SUCCESS;
                            }
                            else if (_apiResp.data[0].transactionStatus == false)
                            {
                                response.Statuscode = RechargeRespType.FAILED;
                                response.LiveID = _apiResp.data[0].transactionStatusMessage;
                            }
                            else
                            {
                                response.Statuscode = RechargeRespType.PENDING;
                            }
                        }
                    }
                    else
                    {
                        if (_apiResp.statusCode == 0)
                        {
                            response.Statuscode = RechargeRespType.FAILED;
                            response.LiveID = ErrorCodes.CanceledByUser;
                            response.Msg = _apiResp.message;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPSStatusCheck",
                    Error = ex.Message,
                });
                response.Msg = ErrorCodes.TempError;
                MGResp = ex.Message + "_" + MGResp;
            }
            response.Req = AEPSStatusCheckURL + "?" + JsonConvert.SerializeObject(fingpayReq);
            response.Resp = MGResp;
            return response;
        }
    }
}