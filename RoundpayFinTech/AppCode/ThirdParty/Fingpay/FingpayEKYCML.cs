using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Fingpay
{
    public partial class FingpayML
    {
        public OutletAPIStatusUpdate SendOTP(ValidateAPIOutletResp _ValidateAPIOutletResp, FingpayAPISetting FPSetting)
        {
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var Req = string.Empty;
            var Resp = string.Empty;
            try
            {
                string _URL = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/sendotp";
                var req = new
                {
                    superMerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                    merchantLoginId = "FP" + _ValidateAPIOutletResp.OutletID,
                    transactionType = "EKY",
                    mobileNumber = _ValidateAPIOutletResp.MobileNo,
                    aadharNumber = _ValidateAPIOutletResp.AADHAR,
                    panNumber = _ValidateAPIOutletResp.PAN,
                    matmSerialNumber = "",
                    latitude = Convert.ToDouble((_ValidateAPIOutletResp.Latlong ?? string.Empty).Split(',')[0].PadRight(10, '0')),
                    longitude = Convert.ToDouble((_ValidateAPIOutletResp.Latlong ?? string.Empty).Split(',')[1].PadRight(10, '0'))
                };
                var TIMESTAMP = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = _ValidateAPIOutletResp.TransactionID;

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
                    var apiResp = JsonConvert.DeserializeObject<FingpayOTPResp>(Resp);
                    if (apiResp != null)
                    {
                        if (apiResp.status)
                        {
                            _resp.Statuscode = ErrorCodes.One;
                            _resp.Msg = nameof(ErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            _resp.APIReferenceID = apiResp.data.primaryKeyId.ToString();
                            _resp.APIHash = apiResp.data.encodeFPTxnId;
                            _resp.IsOTPRequired = true;
                        }
                        else
                        {
                            _resp.Statuscode = ErrorCodes.Minus1;
                            _resp.Msg = apiResp.message;
                            if (apiResp.statusCode == 10029) {
                                _resp.IsEKYCAlreadyVerified = true;
                                _resp.Statuscode = ErrorCodes.One;
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
                    FuncName = "SendOTP",
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
                Method = "FingpayML.SendOTP",
                Request = Req,
                Response = Resp

            });
            #endregion
            return _resp;
        }
        public OutletAPIStatusUpdate ValidateOTP(ValidateAPIOutletResp _ValidateAPIOutletResp, FingpayAPISetting FPSetting)
        {
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var Req = string.Empty;
            var Resp = string.Empty;
            try
            {
                string _URL = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/validateotp";
                var req = new
                {
                    superMerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                    merchantLoginId = "FP" + _ValidateAPIOutletResp.OutletID,
                    otp = _ValidateAPIOutletResp.OTP,
                    primaryKeyId = _ValidateAPIOutletResp.APIReferenceID,
                    encodeFPTxnId=_ValidateAPIOutletResp.APIHash
                };
                var TIMESTAMP = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = _ValidateAPIOutletResp.TransactionID;

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
                    var apiResp = JsonConvert.DeserializeObject<FingpayOTPResp>(Resp);
                    if (apiResp != null)
                    {
                        if (apiResp.status)
                        {
                            _resp.Statuscode = ErrorCodes.One;
                            _resp.Msg = ErrorCodes.OTPVerifiedAndBioMetricRequired;
                            _resp.APIReferenceID = apiResp.data.primaryKeyId.ToString();
                            _resp.APIHash = apiResp.data.encodeFPTxnId;
                            _resp.IsBioMetricRequired = true;
                        }
                        else
                        {
                            _resp.Statuscode = ErrorCodes.Minus1;
                            _resp.Msg = apiResp.message;
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
                    FuncName = "SendOTP",
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
                Method = "FingpayML.SendOTP",
                Request = Req,
                Response = Resp

            });
            #endregion
            return _resp;
        }
        public OutletAPIStatusUpdate ResendOTP(ValidateAPIOutletResp _ValidateAPIOutletResp, FingpayAPISetting FPSetting)
        {
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var Req = string.Empty;
            var Resp = string.Empty;
            try
            {
                string _URL = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/resendotp";
                var req = new
                {
                    superMerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                    merchantLoginId = "FP" + _ValidateAPIOutletResp.OutletID,
                    otp = _ValidateAPIOutletResp.OTP,
                    primaryKeyId = _ValidateAPIOutletResp.APIReferenceID,
                    encodeFPTxnId = _ValidateAPIOutletResp.APIHash
                };
                var TIMESTAMP = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                var SessionKey = HashEncryption.O.GenerateSessionKey();
                var jbytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
                var Sbytes = Encoding.UTF8.GetBytes(SessionKey);
                var Hash = HashEncryption.O.GenerateSha256Hash(jbytes);
                var EncryptUsingSessionKey = HashEncryption.O.EncryptUsingSessionKey(Sbytes, jbytes);
                var EncryptUsintPublicKey = HashEncryption.O.EncryptUsingPublicKey(Sbytes, DOCType.FingpayCertificatePath);
                var deviceIMEI = _ValidateAPIOutletResp.TransactionID;

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
                    var apiResp = JsonConvert.DeserializeObject<FingpayOTPResp>(Resp);
                    if (apiResp != null)
                    {
                        if (apiResp.status)
                        {
                            _resp.Statuscode = ErrorCodes.One;
                            _resp.Msg = ErrorCodes.OTPVerifiedAndBioMetricRequired;
                            _resp.APIReferenceID = apiResp.data.primaryKeyId.ToString();
                            _resp.APIHash = apiResp.data.encodeFPTxnId;
                            _resp.IsBioMetricRequired = true;
                        }
                        else
                        {
                            _resp.Statuscode = ErrorCodes.One;
                            _resp.Msg = apiResp.message;
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
                    FuncName = "ResendOTP",
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
                Method = "FingpayML.ResendOTP",
                Request = Req,
                Response = Resp

            });
            #endregion
            return _resp;
        }
        public OutletAPIStatusUpdate BioMetricEKYCRequest(ValidateAPIOutletResp _ValidateAPIOutletResp, FingpayAPISetting FPSetting)
        {            
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var Req = string.Empty;
            var Resp = string.Empty;
            var pidData = _ValidateAPIOutletResp.pidData;
            try
            {
                string _URL = "https://fpekyc.tapits.in/fpekyc/api/ekyc/merchant/biometric";
                string nationalBankIdentificationNumber = null;
                var req = new
                {
                    superMerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                    merchantLoginId = "FP" + _ValidateAPIOutletResp.OutletID,
                    primaryKeyId = _ValidateAPIOutletResp.APIReferenceID,
                    encodeFPTxnId = _ValidateAPIOutletResp.APIHash,
                    requestRemarks="For Biometric authentication "+ (_ValidateAPIOutletResp.Name??string.Empty),
                    cardnumberORUID = new {
                        nationalBankIdentificationNumber= nationalBankIdentificationNumber,
                        indicatorforUID="0",
                        adhaarNumber= _ValidateAPIOutletResp.AADHAR
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
                    },
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
                    var apiResp = JsonConvert.DeserializeObject<FingpayOTPResp>(Resp);
                    if (apiResp != null)
                    {
                        if (apiResp.status)
                        {
                            _resp.Statuscode = ErrorCodes.One;
                            _resp.Msg = "EKYC done successfully";
                        }
                        else
                        {
                            _resp.Statuscode = ErrorCodes.Minus1;
                            _resp.Msg = apiResp.message;
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
                    FuncName = "BioMetricEKYCRequest",
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
                Method = "FingpayML.BioMetricEKYCRequest",
                Request = Req,
                Response = Resp

            });
            #endregion
            return _resp;
        }
        public void StatusCheckAPI()
        {
            string _URL = "https://fpekyc.tapits.in/fpekyc/api/ekyc/status/check";
        }
    }
}
