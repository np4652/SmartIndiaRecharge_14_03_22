using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Icici;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class ICICIPayoutML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly ICICIPayoutAppSetting appSetting;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private const string PublicKeyFilePath = "Image/ICICI/ICICI_PUBLIC_CERT_PROD.TXT";
        private const string PrivateKeyFilePath = "Image/ICICI/prod_privatekey.txt";
        private const string PublicKeyFilePathUPI = "Image/ICICI/icici_upi_public.txt";
        public const string PrivateKeyFilePathUPI = "Image/ICICI/icici_upi_private.pfx";
        //public const string PrivateKeyFilePathUPI_TXT = "Image/ICICI/upi_icici_private.txt";
        //private const string PublicKeyFilePath = "Image/ICICI/ICICI_PUBLIC_CERT_UAT.TXT";
        //private const string PrivateKeyFilePath = "Image/ICICI/uat_privatekey.key";
        public void MakeICICIFileLog(string response, string filename)
        {
            //try
            //{
            //    string path = "Image/ICICI/log/" + filename;
            //    using (StreamWriter file = new StreamWriter(path, true))
            //    {
            //        file.Write(response);
            //    }

            //}
            //catch (Exception)
            //{
            //}
        }
        public ICICIPayoutML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            appSetting = AppSetting();
            _APIID = APIID;
        }
        public ICICIPayoutAppSetting AppSetting()
        {
            var setting = new ICICIPayoutAppSetting();
            try
            {
                setting = new ICICIPayoutAppSetting
                {
                    CORPID = Configuration["DMR:ICIPOT:CORPID"],
                    USERID = Configuration["DMR:ICIPOT:USERID"],
                    AGGRNAME = Configuration["DMR:ICIPOT:AGGRNAME"],
                    AGGRID = Configuration["DMR:ICIPOT:AGGRID"],
                    URN = Configuration["DMR:ICIPOT:URN"],
                    ALIASID = Configuration["DMR:ICIPOT:ALIASID"],
                    BaseURL = Configuration["DMR:ICIPOT:BaseURL"],
                    DEBITACC = Configuration["DMR:ICIPOT:DEBITACC"],
                    PAYEENAME = Configuration["DMR:ICIPOT:PAYEENAME"],
                    APIKey = Configuration["DMR:ICIPOT:APIKey"],
                    merchantId = Configuration["DMR:ICIPOT:merchantId"],
                    merchantName = Configuration["DMR:ICIPOT:merchantName"],
                    CollectAPIRequestURL = Configuration["DMR:ICIPOT:CollectAPIRequestURL"],
                    CollectAPIAppStatusCallbackURL = Configuration["DMR:ICIPOT:CollectAPIAppStatusCallbackURL"],
                    CollectAPIRequestURLAPP = Configuration["DMR:ICIPOT:CollectAPIRequestURLAPP"],
                    CollectAPIAppStatusURL = Configuration["DMR:ICIPOT:CollectAPIAppStatusURL"],
                    CollectAPIWebStatusURL = Configuration["DMR:ICIPOT:CollectAPIWebStatusURL"],
                    CollectAPIRefundURL = Configuration["DMR:ICIPOT:CollectAPIRefundURL"],
                    QRIntent = Configuration["DMR:ICIPOT:QRIntent"],
                    terminalId = Configuration["DMR:ICIPOT:terminalId"],
                    merchantVPA = Configuration["DMR:ICIPOT:merchantVPA"],
                    CollectVirtualCode = Configuration["DMR:ICIPOT:CollectVirtualCode"],
                    CollectIFSC = Configuration["DMR:ICIPOT:CollectIFSC"],
                    CollectBranch = Configuration["DMR:ICIPOT:CollectBranch"],
                    CollectBeneName = Configuration["DMR:ICIPOT:CollectBeneName"],
                    HYPTO_VerificationURL = Configuration["DMR:ICIPOT:HYPTO_VerificationURL"],
                    HYPTO_VerifyAuth = Configuration["DMR:ICIPOT:HYPTO_VerifyAuth"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ICICIPayoutAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }

        public CollectUPPayResponse CollectPayRequestWeb(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = string.Empty, EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICICollectPayReq
                    {
                        payerVa = request.UPIID,
                        merchantId = appSetting.merchantId,
                        merchantName = appSetting.merchantName,
                        subMerchantId = appSetting.merchantId,
                        subMerchantName = appSetting.merchantName,
                        terminalId = appSetting.terminalId,
                        merchantTranId = "TID" + request.TID,
                        billNumber = "TID" + request.TID,
                        amount = Convert.ToString(request.Amount) ,
                        collectByDate = DateTime.Now.AddHours(4).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                        note = "collect-pay-request"
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIRequestURL.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);
                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, "TID" + request.TID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICollectPayRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        try
                        {
                            byte[] dataOutput = Convert.FromBase64String(response);
                            responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);

                        }
                        catch (Exception ex)
                        {
                            new ProcPageErrorLog(_dal).Call(new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "DecryptUsingPrivatePFXKey",
                                Error = ex.Message,
                                LoginTypeID = LoginType.ApplicationUser,
                                UserId = request.UserID
                            });
                        }
                        if (!Validate.O.ValidateJSON(responseDecrypt ?? string.Empty) && (responseDecrypt ?? string.Empty).Contains("{"))
                        {
                            int index = responseDecrypt.IndexOf("{");
                            responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                        }
                        if (Validate.O.ValidateJSON(responseDecrypt ?? string.Empty))
                        {
                            _apiRes = JsonConvert.DeserializeObject<ICICICollectPayRes>(responseDecrypt);
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.Success != null || _apiRes.success != null)
                    {
                        if (((_apiRes.Success ?? string.Empty).Equals("true") || (_apiRes.success ?? string.Empty).Equals("true")) && _apiRes.response == "92")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = "Transaction Initiated";
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BankRRN = _apiRes.BankRRN;
                            res.VendorID = _apiRes.refId;
                        }
                        else if (_apiRes.response == "5009")
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Bank Down-ICICI";
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayRequestWeb",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    });
                }

                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, ((EncryptedData ?? string.Empty) + "||" + (response ?? string.Empty)), RequestMode.APPS, true, request.Amount, res.VendorID);
            }
            return res;

        }
        public CollectUPPayResponse CollectPayRequestApp(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICICollectPayReqQR
                    {
                        merchantId = appSetting.merchantId,
                        terminalId = appSetting.terminalId,
                        merchantTranId = "TID" + request.TID,
                        billNumber = "TID" + request.TID,
                        amount = request.Amount + ".00"
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIRequestURLAPP.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);

                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, "TID" + request.TID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICollectPayRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);

                        if (!string.IsNullOrEmpty(responseDecrypt))
                        {
                            _apiRes = JsonConvert.DeserializeObject<ICICICollectPayRes>(responseDecrypt);
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.Success != null || _apiRes.success != null)
                    {
                        if (((_apiRes.Success ?? string.Empty).Equals("true") || (_apiRes.success ?? string.Empty).Equals("true")) && _apiRes.response == "0")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BankRRN = _apiRes.BankRRN;
                            res.VendorID = _apiRes.refId;
                        }
                        else if (_apiRes.response == "5009")
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Bank Down_ICICI";
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayRequestApp",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                res.MerchantVPA = appSetting.merchantVPA;
                res.CommonStr2 = appSetting.terminalId;
                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, ((EncryptedData ?? string.Empty) + "||" + (response ?? string.Empty)), RequestMode.APPS, true, request.Amount, res.VendorID);
            }
            return res;

        }
        public CollectUPPayResponse CollectPayRequestAppQR(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICICollectPayReqQR
                    {
                        merchantId = appSetting.merchantId,
                        terminalId = appSetting.terminalId,
                        merchantTranId = request.TransactionID,
                        billNumber = request.TransactionID,
                        amount = request.Amount + ".00"
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIRequestURLAPP.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);

                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, request.TransactionID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICollectPayRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);

                        if (!string.IsNullOrEmpty(responseDecrypt))
                        {
                            _apiRes = JsonConvert.DeserializeObject<ICICICollectPayRes>(responseDecrypt);
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.Success != null || _apiRes.success != null)
                    {
                        if (((_apiRes.Success ?? string.Empty).Equals("true") || (_apiRes.success ?? string.Empty).Equals("true")) && _apiRes.response == "0")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.BankRRN = _apiRes.BankRRN;
                            res.VendorID = _apiRes.refId;
                        }
                        else if (_apiRes.response == "5009")
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Bank Down_ICICI";
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }

                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayRequestAppQR",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                res.MerchantVPA = appSetting.merchantVPA;
                res.CommonStr2 = appSetting.terminalId;
                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, ((EncryptedData ?? string.Empty) + "||" + (response ?? string.Empty)), RequestMode.APPS, true, request.Amount, res.VendorID);
            }
            return res;

        }
        public CollectUPPayResponse CollectPayStatusCheckWeb(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICCollectStatusReq
                    {
                        merchantId = appSetting.merchantId,
                        subMerchantId = appSetting.merchantId,
                        terminalId = appSetting.terminalId,
                        merchantTranId = "TID" + request.TID
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIWebStatusURL.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);

                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, "TID" + request.TID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICollectPayRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);
                        _apiRes = JsonConvert.DeserializeObject<ICICICollectPayRes>(responseDecrypt);
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.Success != null || _apiRes.success != null)
                    {
                        if ((_apiRes.Success ?? string.Empty).Equals("true") || (_apiRes.success ?? string.Empty).Equals("true"))
                        {
                            res.BankRRN = _apiRes.OriginalBankRRN;
                            if (_apiRes.response == "0")
                            {
                                if (_apiRes.status == "SUCCESSS" || _apiRes.status == "SUCCESS")
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.refId;
                                }
                                if (_apiRes.status.Contains("FAIL"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                    res.VendorID = _apiRes.refId;
                                }
                            }
                            else if (_apiRes.response != "99")
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                res.VendorID = _apiRes.refId;
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayStatusCheckWeb",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }

                res.Resp = response;
                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, (EncryptedData ?? string.Empty) + "||" + (response ?? string.Empty), RequestMode.APPS, false, request.Amount, res.VendorID);
            }
            return res;

        }
        public CollectUPPayResponse CollectPayStatusCheckApp(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICCollectStatusReq
                    {
                        merchantId = appSetting.merchantId,
                        subMerchantId = appSetting.merchantId,
                        terminalId = appSetting.terminalId,
                        merchantTranId = "TID" + request.TID
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIAppStatusURL.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);

                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, "TID" + request.TID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICollectPayRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);

                        if (!string.IsNullOrEmpty(responseDecrypt))
                        {
                            _apiRes = JsonConvert.DeserializeObject<ICICICollectPayRes>(responseDecrypt);
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.Success != null || _apiRes.success != null)
                    {
                        if ((_apiRes.Success ?? string.Empty).Equals("true") || (_apiRes.success ?? string.Empty).Equals("true"))
                        {
                            res.BankRRN = _apiRes.OriginalBankRRN;
                            if (_apiRes.response == "0")
                            {
                                if (_apiRes.status == "SUCCESSS")
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.refId;
                                }
                                if (_apiRes.status.Contains("FAIL"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                    res.VendorID = _apiRes.refId;
                                }
                            }
                            else if (_apiRes.response != "99")
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                res.VendorID = _apiRes.refId;
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.Message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.Message;
                            res.VendorID = _apiRes.refId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayStatusCheckApp",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                res.Resp = response;
                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, (EncryptedData ?? string.Empty + "||" + response ?? string.Empty), RequestMode.APPS, false, request.Amount, res.VendorID);
            }
            return res;

        }

        public CollectUPPayResponse CollectPayCallbackStatusCheck(ColllectUPPayReqModel request, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var _Savereq = string.Empty;
            if (appSetting != null)
            {
                string response = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICUPIStatusReq
                    {
                        merchantId = appSetting.merchantId,
                        subMerchantId = appSetting.merchantId,
                        terminalId = appSetting.terminalId,
                        merchantTranId = "TID" + request.TID,
                        transactionType = request.StatusCheckType,
                        BankRRN = request.LiveID
                    };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePathUPI);
                    var URL = appSetting.CollectAPIAppStatusCallbackURL.Replace("<MerchantID>", appSetting.merchantId);
                    _Savereq = URL + "?" + JsonConvert.SerializeObject(iCICRequestModel);

                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, iCICRequestModel, null, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    MakeICICIFileLog(response, "TID" + request.TID);
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICICallBackRes();
                    if (!string.IsNullOrEmpty(response))
                    {
                        if (!response.Contains("Internal Server Error"))
                        {
                            byte[] dataOutput = Convert.FromBase64String(response);
                            responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);

                            if (!string.IsNullOrEmpty(responseDecrypt))
                            {
                                _apiRes = JsonConvert.DeserializeObject<ICICICallBackRes>(responseDecrypt);
                            }
                            response = response + "/AfterDecrypt:" + responseDecrypt;
                        }
                        else
                        {
                            res.CommonStr = "ISERR";
                        }

                    }
                    if (_apiRes.success != null)
                    {
                        res.CommonStr4 = _apiRes.Amount;
                        if ((_apiRes.success ?? string.Empty).Equals("true"))
                        {
                            res.BankRRN = _apiRes.OriginalBankRRN;
                            if (_apiRes.response == "0")
                            {
                                if (_apiRes.status.Equals("SUCCESS") || _apiRes.status.Equals("SUCCESSS"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                if (_apiRes.status.Contains("FAIL"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                    res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                                }
                            }
                            else if (_apiRes.response != "99")
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.BankRRN = _apiRes.OriginalBankRRN;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "CollectPayCallbackStatusCheck",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                res.Resp = response;
                savePGTransactionLog(_APIID, request.TID, _Savereq, request.TransactionID, (EncryptedData ?? string.Empty) + "||" + (response ?? string.Empty), RequestMode.APPS, false, request.Amount, res.VendorID);
            }
            return res;

        }

        //ProcGetSenderLimit
        public async Task<ResponseStatus> CheckSender(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSNE;
                    res.CommonInt = ErrorCodes.One;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)await procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = _APIID
                    }).ConfigureAwait(false);
                    res.CommonStr = (senderLimit.SenderLimit - senderLimit.LimitUsed).ToString();
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSLS;
                    res.CommonStr2 = senderRequest.Name;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSNE;
                    res.CommonInt = ErrorCodes.One;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public ResponseStatus CreateSender(CreateSen _req)
        {
            var dbres = new SenderInfo();
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                _req.senderRequest.UserID = _req.dMTReq.UserID;
                dbres = (new ProcUpdateSender(_dal).Call(_req.senderRequest)) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTOSS;
                    res.CommonInt = ErrorCodes.One;
                    res.CommonStr = dbres.OTP;
                    res.CommonInt2 = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = _req.dMTReq.LT,
                    UserId = _req.dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.dMTReq.ApiID,
                Method = "CreateSender",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = JsonConvert.SerializeObject(_req),
                Response = JsonConvert.SerializeObject(res),
                SenderNo = _req.dMTReq.SenderNO,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public BeniRespones GetBeneficiary(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var resDB = (new ProcGetBenificiary(_dal).Call(_req)) as BenificiaryModel;
                res.Statuscode = resDB.Statuscode;
                res.Msg = resDB.Msg;
                if (resDB != null && resDB.Statuscode == ErrorCodes.One)
                {
                    var ListBeni = new List<AddBeni>();
                    if (resDB.benificiaries != null && resDB.benificiaries.Count > 0)
                    {
                        foreach (var r in resDB.benificiaries)
                        {
                            var addBeni = new AddBeni
                            {
                                AccountNo = r._AccountNumber,
                                BankName = r._BankName,
                                IFSC = r._IFSC,
                                BeneName = r._Name,
                                MobileNo = r._BankName,
                                BeneID = r._ID.ToString()
                            };
                            ListBeni.Add(addBeni);
                        }
                    }
                    res.addBeni = ListBeni;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "GetBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = "|" + JsonConvert.SerializeObject(_req),
                Response = "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public ResponseStatus VerifySender(DMTReq _req, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = _req.SenderNO,
                    CommonStr2 = OTP,
                    CommonInt = _req.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "VerifySender",
                RequestModeID = _req.RequestMode,
                Request = JsonConvert.SerializeObject(_req),
                Response = JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus CreateBeneficiary(AddBeni addBeni, DMTReq _req)
        {
            string response = "", request = "";
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = addBeni.SenderMobileNo,
                    _Name = addBeni.BeneName,
                    _AccountNumber = addBeni.AccountNo,
                    _MobileNo = addBeni.MobileNo,
                    _IFSC = addBeni.IFSC,
                    _BankName = addBeni.BankName,
                    _EntryBy = _req.UserID,
                    _VerifyStatus = 1,
                    _BankID = addBeni.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    return res;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CreateBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public async Task<DMRTransactionResponse> ICICIPayout(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;

            if (appSetting != null)
            {
                string response = "", request = "", EncryptedData = string.Empty;
                try
                {
                    var iCICRequestModel = new ICICIPayoutRequest
                    {
                        AGGRID = appSetting.AGGRID,
                        CORPID = appSetting.CORPID,
                        USERID = appSetting.USERID,
                        AGGRNAME = appSetting.AGGRNAME,
                        URN = appSetting.URN,
                        UNIQUEID = res.TID.ToString().PadRight(16, 'T'),
                        AMOUNT = sendMoney.Amount,
                        CREDITACC = sendMoney.AccountNo,
                        DEBITACC = appSetting.DEBITACC,
                        CURRENCY = "INR",
                        IFSC = sendMoney.IFSC,
                        PAYEENAME = appSetting.PAYEENAME,
                        REMARKS = "Payout"
                    };
                    //Enter RTG for RTGS, RGS for NEFT ,IFS for IMPS ,OWN for Own to Own & TPA for Own to external payments.  For Virtual A/c payments Txn_type should be "VAP" & "RGS" and IFSC should be ICIC0000103, ICIC0000104 & ICIC0000106 depending on the client codes created for the service of virtual account number based collection. This is communicated during setup of this service for any client. IMPS & RTGS txn will not allowed for virtual payments.
                    if (_req.ChanelType == PaymentMode_.Payout_IMPS)
                    {
                        iCICRequestModel.TXNTYPE = "IFS";
                    }
                    //else if (_req.ChanelType == PaymentMode_.Payout_Neft)
                    //{
                    //    iCICRequestModel.TXNTYPE = "RGS";
                    //}
                    else if (_req.ChanelType == PaymentMode_.Payout_RTGS)
                    {
                        iCICRequestModel.TXNTYPE = "RTG";
                        if (sendMoney.IFSC.In("ICIC0000103", "ICIC0000104"))
                        {
                            iCICRequestModel.TXNTYPE = "RGS";
                        }
                    }
                    //!iCICRequestModel.TXNTYPE.Equals("RGS") &&
                    if (sendMoney.IFSC.In("ICIC0000103", "ICIC0000104", "ICIC0000105", "ICIC0000106"))
                    {
                        iCICRequestModel.TXNTYPE = "VAP";
                    }
                    if (!iCICRequestModel.TXNTYPE.In("RGS", "VAP") && sendMoney.IFSC.StartsWith("ICIC"))
                    {
                        iCICRequestModel.TXNTYPE = "TPA";
                        iCICRequestModel.IFSC = "ICIC0000011";
                    }
                    //if (sendMoney.IFSC.In("CIC0000103", "ICIC0000104", "ICIC0000105", "ICIC0000106"))
                    //{
                    //    iCICRequestModel.TXNTYPE = "VAP";
                    //}
                    if (!sendMoney.IFSC.StartsWith("ICIC") && _req.ChanelType == PaymentMode_.Payout_Neft)
                    {
                        iCICRequestModel.TXNTYPE = "RGS";
                    }
                    var _parameterList = new Dictionary<string, object>
                   {
                        { nameof(iCICRequestModel.CORPID), iCICRequestModel.CORPID },
                        { nameof(iCICRequestModel.USERID), iCICRequestModel.USERID },
                        { nameof(iCICRequestModel.AGGRNAME), iCICRequestModel.AGGRNAME },
                        { nameof(iCICRequestModel.AGGRID), iCICRequestModel.AGGRID },
                        { nameof(iCICRequestModel.URN), iCICRequestModel.URN },
                        { nameof(iCICRequestModel.UNIQUEID), iCICRequestModel.UNIQUEID },
                        { nameof(iCICRequestModel.CREDITACC), iCICRequestModel.CREDITACC },
                        { nameof(iCICRequestModel.AMOUNT), iCICRequestModel.AMOUNT+".00" },
                        { nameof(iCICRequestModel.IFSC), iCICRequestModel.IFSC },
                        { nameof(iCICRequestModel.DEBITACC), iCICRequestModel.DEBITACC },
                        { nameof(iCICRequestModel.CURRENCY), "INR" },
                        { nameof(iCICRequestModel.PAYEENAME), iCICRequestModel.PAYEENAME },
                        { nameof(iCICRequestModel.TXNTYPE), iCICRequestModel.TXNTYPE },
                        //{ nameof(iCICRequestModel.WORKFLOW_REQD), 'N' },
                        { nameof(iCICRequestModel.REMARKS), iCICRequestModel.REMARKS }
                   };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePath);
                    var URL = appSetting.BaseURL + "Transaction";
                    request = URL + "?" + JsonConvert.SerializeObject(_parameterList);
                    var headers = new Dictionary<string, string>
                    {
                        { "apikey", appSetting.APIKey }
                    };
                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, _parameterList, headers, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    var responseDecrypt = string.Empty;
                    var _apiRes = new ICICPayoutResponse();
                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        bool IsException = false;
                        try
                        {
                            responseDecrypt = HashEncryption.O.DecryptUsingPrivateKey(dataOutput, PrivateKeyFilePath);
                        }
                        catch (Exception)
                        {
                            IsException = true;
                        }
                        
                        if (!string.IsNullOrEmpty(responseDecrypt) && IsException==false)
                        {
                            var index = responseDecrypt.IndexOf("{\r\n");
                            if (index == -1)
                            {
                                index = responseDecrypt.IndexOf("{");
                            }

                            if (index > -1)
                            {
                                responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                            }
                        }

                        if (!Validate.O.ValidateJSON(responseDecrypt) || IsException)
                        {
                            responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);
                            if (!Validate.O.ValidateJSON(responseDecrypt ?? string.Empty) && (responseDecrypt ?? string.Empty).Contains("{"))
                            {
                                int index = responseDecrypt.IndexOf("{");
                                responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                            }
                        }


                        if (Validate.O.ValidateJSON(responseDecrypt))
                        {
                            _apiRes = JsonConvert.DeserializeObject<ICICPayoutResponse>(responseDecrypt);
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }
                    if (_apiRes.STATUS != null)
                    {
                        if (_apiRes.STATUS.Equals("SUCCESS"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            res.LiveID = _apiRes.UTRNUMBER ?? string.Empty;
                            res.VendorID = _apiRes.REQID ?? string.Empty;
                        }
                        else if (_apiRes.STATUS.Equals("FAILURE") && ((_apiRes.RESPONSECODE).GetType() == "System.String".GetType() ? _apiRes.RESPONSECODE != "954" : false))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = (_apiRes.message ?? string.Empty).Contains("insuf") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                        else
                        {
                            res.Msg = _apiRes.MESSAGE;
                        }
                    }
                    else
                    {
                        if (_apiRes.response.Equals("FAILURE"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = (_apiRes.message ?? string.Empty).Contains("insuf") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : _apiRes.message;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "SendMoney",
                        Error = ex.Message,
                        LoginTypeID = _req.LT,
                        UserId = _req.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }

                var dMTReq = new DMTReqRes
                {
                    APIID = _req.ApiID,
                    Method = "SendMoney",
                    RequestModeID = _req.RequestMode,
                    Request = request + "||" + EncryptedData,
                    Response = response,
                    SenderNo = _req.SenderNO,
                    UserID = _req.UserID,
                    TID = _req.TID
                };
                new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
                res.Request = dMTReq.Request;
                res.Response = dMTReq.Response;
            }
            return res;
        }

        public async Task<string> ICICIRegister()
        {
            string response = "", request = "", EncryptedData = string.Empty;
            if (appSetting != null)
            {
                try
                {
                    var iCICRequestModel = new ICICIPayoutRequest
                    {
                        AGGRID = appSetting.AGGRID,
                        CORPID = appSetting.CORPID,
                        USERID = appSetting.USERID,
                        AGGRNAME = appSetting.AGGRNAME,
                        URN = appSetting.URN,
                        ALIASID = appSetting.ALIASID
                    };


                    var _parameterList = new Dictionary<string, object>
                   {
                        { nameof(iCICRequestModel.CORPID), iCICRequestModel.CORPID },
                        { nameof(iCICRequestModel.USERID), iCICRequestModel.USERID },
                        { nameof(iCICRequestModel.AGGRNAME), iCICRequestModel.AGGRNAME },
                        { nameof(iCICRequestModel.AGGRID), iCICRequestModel.AGGRID },
                        { nameof(iCICRequestModel.URN), iCICRequestModel.URN },
                        { nameof(iCICRequestModel.ALIASID), iCICRequestModel.ALIASID }
                   };
                    string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePath);
                    var URL = appSetting.BaseURL + "Registration";
                    request = URL + "?" + JsonConvert.SerializeObject(_parameterList);
                    var headers = new Dictionary<string, string>
                    {
                        { "apikey", appSetting.APIKey }
                    };
                    var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, _parameterList, headers, KeyPath).Result;
                    response = webRequest.Response;
                    EncryptedData = webRequest.EncryptedData;
                    var responseDecrypt = string.Empty;

                    if (!string.IsNullOrEmpty(response))
                    {
                        byte[] dataOutput = Convert.FromBase64String(response);
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivateKey(dataOutput, PrivateKeyFilePath);

                        if (!string.IsNullOrEmpty(responseDecrypt))
                        {
                            var index = responseDecrypt.IndexOf("{\r\n");
                            if (index == -1)
                            {
                                index = responseDecrypt.IndexOf("{");
                            }

                            if (index > -1)
                            {
                                responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                                // _apiRes = JsonConvert.DeserializeObject<ICICPayoutResponse>(responseDecrypt);
                            }
                        }
                        response = response + "/AfterDecrypt:" + responseDecrypt;
                    }

                }
                catch (Exception ex)
                {
                    response = ex.Message + "|" + response;
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "ICICIRegister",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = 1
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }

            }
            return request + "||" + EncryptedData ?? string.Empty + "||" + response ?? string.Empty;
        }

        //public async Task<DMRTransactionResponse> TransactionQuery(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        //{
        //    res.Statuscode = RechargeRespType.PENDING;
        //    res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
        //    res.ErrorCode = ErrorCodes.Request_Accpeted;
        //    res.LiveID = res.Msg;

        //    if (appSetting != null)
        //    {
        //        string response = "", request = "";
        //        try
        //        {
        //            SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
        //            var iciciModel = new ICICIReqModel
        //            {
        //                Amount = sendMoney.Amount.ToString(),
        //                BeneAccNo = sendMoney.AccountNo,
        //                BeneIFSC = sendMoney.IFSC,
        //                RemName = senderRequest.Name,
        //                RemMobile = _req.SenderNO,
        //                TranRefNo = res.TID.ToString(),
        //                RetailerCode = "rcode",
        //                PaymentRef = "FTTransferP2A" + res.TID,
        //                TransactionDate = DateTime.Now.ToString("yyyyMMddhhmmss"),
        //                PassCode = appSetting.PassCode
        //            };
        //            var sb = new StringBuilder();

        //            sb.Append(nameof(iciciModel.TranRefNo));
        //            sb.Append("=");
        //            sb.Append(iciciModel.TranRefNo);
        //            sb.Append("&");
        //            sb.Append(nameof(iciciModel.PassCode));
        //            sb.Append("=");
        //            sb.Append(iciciModel.PassCode);

        //            var URL = appSetting.BaseURL + "imps-web-bc/api/transaction/bc/" + appSetting.BC + "/query";
        //            request = URL + "?" + sb.ToString();
        //            response = await AppWebRequest.O.CallUsingHttpWebRequest_POSTAsync(URL, sb.ToString(), "text/plain").ConfigureAwait(false);
        //            var _apiRes = new ICICIImpsResponse();
        //            _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, "ImpsResponse", true);
        //            if (Validate.O.IsNumeric(_apiRes.ActCode ?? string.Empty))
        //            {
        //                var ActCode = Convert.ToInt32(_apiRes.ActCode);
        //                if (ActCode == 0)
        //                {
        //                    res.Statuscode = RechargeRespType.SUCCESS;
        //                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
        //                    res.ErrorCode = ErrorCodes.Transaction_Successful;
        //                    res.LiveID = _apiRes.BankRRN ?? string.Empty;
        //                    res.VendorID = _apiRes.TranRefNo ?? string.Empty;
        //                }
        //                else if ((ActCode >= 1 && ActCode <= 10) || (ActCode >= 12 && ActCode <= 15) || (ActCode >= 18 && ActCode <= 21))
        //                {
        //                    res.Statuscode = RechargeRespType.FAILED;
        //                    res.Msg = _apiRes.Response;
        //                    res.ErrorCode = ErrorCodes.Unknown_Error;
        //                    res.LiveID = _apiRes.Response;
        //                }
        //            }
        //            res.Request = request;
        //            res.Response = response;
        //        }
        //        catch (Exception ex)
        //        {
        //            response = ex.Message + "|" + response;
        //            var errorLog = new ErrorLog
        //            {
        //                ClassName = GetType().Name,
        //                FuncName = "TransactionQuery",
        //                Error = ex.Message,
        //                LoginTypeID = _req.LT,
        //                UserId = _req.UserID
        //            };
        //            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //        }
        //        var dMTReq = new DMTReqRes
        //        {
        //            APIID = _req.ApiID,
        //            Method = "TransactionQuery",
        //            RequestModeID = _req.RequestMode,
        //            Request = request,
        //            Response = response,
        //            SenderNo = _req.SenderNO,
        //            UserID = _req.UserID,
        //            TID = _req.TID
        //        };
        //        new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        //    }
        //    return res;
        //}


    }

    public partial class ICICIPayoutML : IMoneyTransferAPIML
    {
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = request.APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.SenderMobile = request.SenderMobile;
                    res.KYCStatus = SenderKYCStatus.ACTIVE;
                    res.SenderName = senderRequest.Name;
                    res.IsNotCheckLimit = senderRequest.IsNotCheckLimit;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = request.SenderMobile,
                    _Name = request.mBeneDetail.BeneName,
                    _AccountNumber = request.mBeneDetail.AccountNo,
                    _MobileNo = request.mBeneDetail.MobileNo,
                    _IFSC = request.mBeneDetail.IFSC,
                    _BankName = request.mBeneDetail.BankName,
                    _EntryBy = request.UserID,
                    _VerifyStatus = 1,
                    _APICode = request.APICode,
                    _BankID = request.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
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
            throw new NotImplementedException();
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            return DMTAPIHelperML.GetBeneficiary(request, _dal, GetType().Name);
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
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = request.SenderMobile,
                    CommonStr2 = request.OTP,
                    CommonInt = request.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
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
                    LoginID = request.UserID,
                    CommonInt = Convert.ToInt32(request.mBeneDetail.BeneID),
                    CommonStr = request.SenderMobile
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
            res = DMTAPIHelperML.AccountVerification(appSetting.HYPTO_VerificationURL, appSetting.HYPTO_VerifyAuth, request.mBeneDetail.AccountNo, request.mBeneDetail.IFSC, request.TID.ToString(), _dal);
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "Verification",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
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
            var beneficiaryModel = new BeneficiaryModel
            {
                ID = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            var senderRequest = new SenderRequest();
            IProcedure _proc = new GetBeneficaryByID(_dal);
            if (request.IsPayout)
            {
                beneficiaryModel.Name = request.mBeneDetail.BeneName;
                senderRequest.MobileNo = request.SenderMobile;
                senderRequest.Name = request.UserName;
            }
            else
            {
                beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            }
            var URL = appSetting.BaseURL + "Transaction";
            string response = "", EncryptedData = string.Empty;
            var headers = new Dictionary<string, string>
                    {
                        { "apikey", appSetting.APIKey }
                    };
            var iCICRequestModel = new ICICIPayoutRequest
            {
                AGGRID = appSetting.AGGRID,
                CORPID = appSetting.CORPID,
                USERID = appSetting.USERID,
                AGGRNAME = appSetting.AGGRNAME,
                URN = appSetting.URN,
                UNIQUEID = request.TID.ToString().PadRight(16, 'T'),
                AMOUNT = request.Amount,
                CREDITACC = request.mBeneDetail.AccountNo,
                DEBITACC = appSetting.DEBITACC,
                CURRENCY = "INR",
                IFSC = request.mBeneDetail.IFSC,
                PAYEENAME = appSetting.PAYEENAME,
                REMARKS = "Payout"
            };

            //Enter RTG for RTGS, RGS for NEFT ,IFS for IMPS ,OWN for Own to Own & TPA for Own to external payments.  For Virtual A/c payments Txn_type should be "VAP" & "RGS" and IFSC should be ICIC0000103, ICIC0000104 & ICIC0000106 depending on the client codes created for the service of virtual account number based collection. This is communicated during setup of this service for any client. IMPS & RTGS txn will not allowed for virtual payments.

            if (request.TransMode.Equals("IMPS"))
            {
                iCICRequestModel.TXNTYPE = "IFS";
            }
            //else if (request.TransMode.Equals("NEFT"))
            //{
            //    iCICRequestModel.TXNTYPE = "RGS";
            //}
            else if (request.TransMode == "RTGS")
            {
                iCICRequestModel.TXNTYPE = "RTG";
                if (request.mBeneDetail.IFSC.In("ICIC0000103", "ICIC0000104"))
                {
                    iCICRequestModel.TXNTYPE = "RGS";
                }
            }
            //!iCICRequestModel.TXNTYPE.Equals("RGS") &&
            if (request.mBeneDetail.IFSC.In("ICIC0000103", "ICIC0000104", "ICIC0000105", "ICIC0000106"))
            {
                iCICRequestModel.TXNTYPE = "VAP";
            }
            if (!iCICRequestModel.TXNTYPE.In("RGS", "VAP") && request.mBeneDetail.IFSC.StartsWith("ICIC"))
            {
                iCICRequestModel.TXNTYPE = "TPA";
                iCICRequestModel.IFSC = !request.mBeneDetail.IFSC.In("ICIC0000104", "ICIC0000103", "ICIC0000105", "ICIC0000106") ? "ICIC0000011" : request.mBeneDetail.IFSC;
            }
            if (!request.mBeneDetail.IFSC.StartsWith("ICIC") && request.TransMode == "NEFT")
            {
                iCICRequestModel.TXNTYPE = "RGS";
            }
            //if (request.mBeneDetail.IFSC.In("CIC0000103", "ICIC0000104", "ICIC0000105", "ICIC0000106"))
            //{
            //    iCICRequestModel.TXNTYPE = "VAP";
            //}

            var _parameterList = new Dictionary<string, object>
            {
                { nameof(iCICRequestModel.CORPID), iCICRequestModel.CORPID },
                { nameof(iCICRequestModel.USERID), iCICRequestModel.USERID },
                { nameof(iCICRequestModel.AGGRNAME), iCICRequestModel.AGGRNAME },
                { nameof(iCICRequestModel.AGGRID), iCICRequestModel.AGGRID },
                { nameof(iCICRequestModel.URN), iCICRequestModel.URN },
                { nameof(iCICRequestModel.UNIQUEID), iCICRequestModel.UNIQUEID },
                { nameof(iCICRequestModel.CREDITACC), iCICRequestModel.CREDITACC },
                { nameof(iCICRequestModel.AMOUNT), iCICRequestModel.AMOUNT+".00" },
                { nameof(iCICRequestModel.IFSC), iCICRequestModel.IFSC },
                { nameof(iCICRequestModel.DEBITACC), iCICRequestModel.DEBITACC },
                { nameof(iCICRequestModel.CURRENCY), "INR" },
                { nameof(iCICRequestModel.PAYEENAME), iCICRequestModel.PAYEENAME },
                { nameof(iCICRequestModel.TXNTYPE), iCICRequestModel.TXNTYPE },
                //{ nameof(iCICRequestModel.WORKFLOW_REQD), 'N' },
                { nameof(iCICRequestModel.REMARKS), iCICRequestModel.REMARKS }
            };

            try
            {
                string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), PublicKeyFilePath);
                var webRequest = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, _parameterList, headers, KeyPath).Result;
                response = webRequest.Response;
                EncryptedData = webRequest.EncryptedData;

                var responseDecrypt = string.Empty;
                var _apiRes = new ICICPayoutResponse();
                if (!string.IsNullOrEmpty(response))
                {
                    byte[] dataOutput = Convert.FromBase64String(response);
                    bool IsException = false;
                    try
                    {
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivateKey(dataOutput, PrivateKeyFilePath);
                    }
                    catch (Exception)
                    {
                        IsException = true;
                    }
                   

                    if (!string.IsNullOrEmpty(responseDecrypt) && IsException==false)
                    {
                        var index = responseDecrypt.IndexOf("{\r\n");
                        if (index == -1)
                        {
                            index = responseDecrypt.IndexOf("{");
                        }

                        if (index > -1)
                        {
                            responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                        }
                    }
                    if (!Validate.O.ValidateJSON(responseDecrypt) || IsException)
                    {
                        responseDecrypt = HashEncryption.O.DecryptUsingPrivatePFXKey(dataOutput, PrivateKeyFilePathUPI);
                        if (!Validate.O.ValidateJSON(responseDecrypt ?? string.Empty) && (responseDecrypt ?? string.Empty).Contains("{"))
                        {
                            int index = responseDecrypt.IndexOf("{");
                            responseDecrypt = responseDecrypt.Substring(index, responseDecrypt.Length - index).Replace("\r\n", "").Replace(": ", ":");
                        }
                    }
                    if (Validate.O.ValidateJSON(responseDecrypt))
                    {
                        _apiRes = JsonConvert.DeserializeObject<ICICPayoutResponse>(responseDecrypt);
                    }
                    response = response + "/AfterDecrypt:" + responseDecrypt;
                }
                if (_apiRes.STATUS != null)
                {
                    if (_apiRes.STATUS.Equals("SUCCESS"))
                    {
                        res.Statuscode = RechargeRespType.SUCCESS;
                        res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        res.LiveID = _apiRes.UTRNUMBER ?? string.Empty;
                        res.VendorID = _apiRes.REQID ?? string.Empty;
                    }
                    else if (_apiRes.STATUS.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = (_apiRes.MESSAGE ?? string.Empty).Contains("insuf") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : _apiRes.MESSAGE;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                    }
                    else
                    {
                        res.Msg = _apiRes.MESSAGE;
                    }
                }
                else
                {
                    if (_apiRes.response.Equals("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = (_apiRes.message ?? string.Empty).Contains("insuf") ? nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ") : _apiRes.message;
                        res.ErrorCode = ErrorCodes.Unknown_Error;
                        res.LiveID = res.Msg;
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = string.Format("{0}?{1}|{2}", URL, JsonConvert.SerializeObject(_parameterList), JsonConvert.SerializeObject(headers)) + "||" + (EncryptedData ?? string.Empty),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;

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
