using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using paytm;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using Fintech.AppCode.WebRequest;
using System.Text;

namespace RoundpayFinTech.AppCode.ThirdParty.Paytm
{
    public partial class PaytmML
    {
        private readonly IDAL _dal;
        public PaytmML(IDAL dal) => _dal = dal;
        public PGModelForRedirection GeneratePGRequestForWeb(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var paytmPGRequest = new PaytmPGRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(paytmPGRequest.MID), pGTransactionResponse.MerchantID },
                    { nameof(paytmPGRequest.WEBSITE), pGTransactionResponse.ENVCode },
                    { nameof(paytmPGRequest.INDUSTRY_TYPE_ID), pGTransactionResponse.IndustryType },
                    { nameof(paytmPGRequest.CHANNEL_ID), "WEB" },
                    { nameof(paytmPGRequest.ORDER_ID),  pGTransactionResponse.TID.ToString()},
                    { nameof(paytmPGRequest.CUST_ID),  pGTransactionResponse.UserID.ToString()},
                    { nameof(paytmPGRequest.MOBILE_NO),  pGTransactionResponse.MobileNo},
                    { nameof(paytmPGRequest.EMAIL),  pGTransactionResponse.EmailID},
                    { nameof(paytmPGRequest.TXN_AMOUNT),  pGTransactionResponse.Amount.ToString()},
                    { nameof(paytmPGRequest.CALLBACK_URL),  pGTransactionResponse.Domain+"/PGCallback/Paytm"}
                };
                paytmPGRequest.CHECKSUMHASH = CheckSum.generateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
                KeyVals.Add(nameof(paytmPGRequest.CHECKSUMHASH), paytmPGRequest.CHECKSUMHASH);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }
        public PGModelForRedirection GeneratePGRequestForJS(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var paytmPGRequest = new PaytmPGRequest();
            try
            {
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                res.paytmJSRequest = new PaytmJSRequest
                {
                    MID = pGTransactionResponse.MerchantID,
                    Amount = pGTransactionResponse.Amount.ToString(),
                    OrderID = pGTransactionResponse.TID.ToString(),
                    TokenType = "TXN_TOKEN"
                };
                var txnAmount = new Dictionary<string, string> {
                    { "value", pGTransactionResponse.Amount.ToString()},
                    { "currency", "INR"}
                };

                var userInfo = new Dictionary<string, string> {
                    { "custId", "CUST_"+pGTransactionResponse.UserID}
                };
                var body = new Dictionary<string, object> {
                    {"requestType", "Payment" },
                    {"mid",  pGTransactionResponse.MerchantID },
                    {"websiteName", pGTransactionResponse.ENVCode},
                    {"orderId", pGTransactionResponse.TID.ToString() },
                    {"txnAmount", txnAmount },
                    {"userInfo", userInfo },
                    { "callbackUrl", pGTransactionResponse.Domain+"/PGCallback/Paytm"},
                };
                res.paytmJSRequest.CallbackUrl = Convert.ToString(body["callbackUrl"]);
                paytmPGRequest.CHECKSUMHASH = CheckSum.generateSignature(JsonConvert.SerializeObject(body), pGTransactionResponse.MerchantKey);
                var head = new Dictionary<string, string> {
                    { "signature", paytmPGRequest.CHECKSUMHASH }
                };
                var requestBody = new Dictionary<string, object> {
                    {"body", body },
                    {"head", head }
                };
                string post_data = JsonConvert.SerializeObject(requestBody);
                StringBuilder HitURL = new StringBuilder("{HOST}theia/api/v1/initiateTransaction?mid={MID}&orderId={ORDER_ID}");
                HitURL.Replace("{HOST}", pGTransactionResponse.URL);
                HitURL.Replace("{MID}", pGTransactionResponse.MerchantID);
                HitURL.Replace("{ORDER_ID}", pGTransactionResponse.TID.ToString());
                var responseData = AppWebRequest.O.PostJsonDataUsingHWRTLS(HitURL.ToString(), requestBody, head).Result;
                if (!string.IsNullOrEmpty(responseData))
                {
                    var apiResp = JsonConvert.DeserializeObject<PaytmTokenResponse>(responseData);
                    if (apiResp.body != null)
                    {
                        if (apiResp.body.resultInfo.resultCode.Equals("0000"))
                        {
                            res.paytmJSRequest.Token = apiResp.body.txnToken;
                        }
                    }
                }
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                });
            }

            return res;
        }
        public void GeneratePGRequestForJSApp(PGTransactionResponse pGTransactionResponse, PGModelForApp res, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var paytmPGRequest = new PaytmPGRequest
            {
                MID = pGTransactionResponse.MerchantID,
                WEBSITE = pGTransactionResponse.ENVCode,
                INDUSTRY_TYPE_ID = pGTransactionResponse.IndustryType,
                CHANNEL_ID = "WAP",
                ORDER_ID = pGTransactionResponse.TID.ToString(),
                CUST_ID = pGTransactionResponse.UserID.ToString(),
                MOBILE_NO = pGTransactionResponse.MobileNo,
                EMAIL = pGTransactionResponse.EmailID,
                TXN_AMOUNT = pGTransactionResponse.Amount.ToString(),
                CALLBACK_URL = pGTransactionResponse.Domain + "/PGCallback/Paytm"
            };
            var KeyVals = new Dictionary<string, string>
            {
                { nameof(paytmPGRequest.MID), paytmPGRequest.MID },
                { nameof(paytmPGRequest.WEBSITE),  paytmPGRequest.WEBSITE},
                { nameof(paytmPGRequest.INDUSTRY_TYPE_ID),  paytmPGRequest.INDUSTRY_TYPE_ID},
                { nameof(paytmPGRequest.CHANNEL_ID), paytmPGRequest.CHANNEL_ID },
                { nameof(paytmPGRequest.ORDER_ID),  paytmPGRequest.ORDER_ID},
                { nameof(paytmPGRequest.CUST_ID), paytmPGRequest.CUST_ID },
                { nameof(paytmPGRequest.MOBILE_NO), paytmPGRequest.MOBILE_NO },
                { nameof(paytmPGRequest.EMAIL), paytmPGRequest.EMAIL },
                { nameof(paytmPGRequest.TXN_AMOUNT), paytmPGRequest.TXN_AMOUNT },
                { nameof(paytmPGRequest.CALLBACK_URL), paytmPGRequest.CALLBACK_URL }
            };
            var txnAmount = new Dictionary<string, string> {
                    { "value", pGTransactionResponse.Amount.ToString()},
                    { "currency", "INR"}
                };

            var userInfo = new Dictionary<string, string> {
                    { "custId", "CUST_"+pGTransactionResponse.UserID}
                };
            var body = new Dictionary<string, object> {
                    {"requestType", "Payment" },
                    {"mid",  pGTransactionResponse.MerchantID },
                    {"websiteName", pGTransactionResponse.ENVCode},
                    {"orderId", pGTransactionResponse.TID.ToString() },
                    {"txnAmount", txnAmount },
                    {"userInfo", userInfo },
                    {"callbackUrl", pGTransactionResponse.Domain+"/PGCallback/Paytm"},
                };

            paytmPGRequest.CHECKSUMHASH = CheckSum.generateSignature(JsonConvert.SerializeObject(body), pGTransactionResponse.MerchantKey);
            var head = new Dictionary<string, string> {
                    { "signature", paytmPGRequest.CHECKSUMHASH }
                };
            var requestBody = new Dictionary<string, object> {
                    {"body", body },
                    {"head", head }
                };
            string post_data = JsonConvert.SerializeObject(requestBody);
            StringBuilder HitURL = new StringBuilder("{HOST}theia/api/v1/initiateTransaction?mid={MID}&orderId={ORDER_ID}");
            HitURL.Replace("{HOST}", pGTransactionResponse.URL);
            HitURL.Replace("{MID}", pGTransactionResponse.MerchantID);
            HitURL.Replace("{ORDER_ID}", pGTransactionResponse.TID.ToString());
            var responseData = AppWebRequest.O.PostJsonDataUsingHWRTLS(HitURL.ToString(), requestBody, head).Result;
            if (!string.IsNullOrEmpty(responseData))
            {
                var apiResp = JsonConvert.DeserializeObject<PaytmTokenResponse>(responseData);
                if (apiResp.body != null)
                {
                    if (apiResp.body.resultInfo.resultCode.Equals("0000"))
                    {
                        res.Token = apiResp.body.txnToken;
                    }
                }
            }
            res.requestPTM = paytmPGRequest;
            savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.APPS, true, pGTransactionResponse.Amount, string.Empty);
        }
        public void GeneratePGRequestForApp(PGTransactionResponse pGTransactionResponse, PGModelForApp res, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var paytmPGRequest = new PaytmPGRequest
            {
                MID = pGTransactionResponse.MerchantID,
                WEBSITE = pGTransactionResponse.ENVCode,
                INDUSTRY_TYPE_ID = pGTransactionResponse.IndustryType,
                CHANNEL_ID = "WAP",
                ORDER_ID = pGTransactionResponse.TID.ToString(),
                CUST_ID = pGTransactionResponse.UserID.ToString(),
                MOBILE_NO = pGTransactionResponse.MobileNo,
                EMAIL = pGTransactionResponse.EmailID,
                TXN_AMOUNT = pGTransactionResponse.Amount.ToString(),
                CALLBACK_URL = "https://securegw.paytm.in/theia/paytmCallback?ORDER_ID=" + pGTransactionResponse.TID.ToString()
            };
            var KeyVals = new Dictionary<string, string>
            {
                { nameof(paytmPGRequest.MID), paytmPGRequest.MID },
                { nameof(paytmPGRequest.WEBSITE),  paytmPGRequest.WEBSITE},
                { nameof(paytmPGRequest.INDUSTRY_TYPE_ID),  paytmPGRequest.INDUSTRY_TYPE_ID},
                { nameof(paytmPGRequest.CHANNEL_ID), paytmPGRequest.CHANNEL_ID },
                { nameof(paytmPGRequest.ORDER_ID),  paytmPGRequest.ORDER_ID},
                { nameof(paytmPGRequest.CUST_ID), paytmPGRequest.CUST_ID },
                { nameof(paytmPGRequest.MOBILE_NO), paytmPGRequest.MOBILE_NO },
                { nameof(paytmPGRequest.EMAIL), paytmPGRequest.EMAIL },
                { nameof(paytmPGRequest.TXN_AMOUNT), paytmPGRequest.TXN_AMOUNT },
                { nameof(paytmPGRequest.CALLBACK_URL), paytmPGRequest.CALLBACK_URL }
            };
            paytmPGRequest.CHECKSUMHASH = CheckSum.generateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
            res.requestPTM = paytmPGRequest;
            savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.APPS, true, pGTransactionResponse.Amount, string.Empty);
        }

        public PaytmPGResponse StatusCheckPG(TransactionPGLog transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var payresp = new PaytmPGResponse();
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var paytmPGRequest = new PaytmPGRequest();
            string paytmresp = string.Empty;
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(paytmPGRequest.MID), transactionPGLog.MerchantID },
                    { nameof(paytmPGRequest.ORDER_ID), transactionPGLog.TID.ToString() }
                };
                paytmPGRequest.CHECKSUMHASH = CheckSum.generateCheckSum(transactionPGLog.MerchantKEY, KeyVals);
                KeyVals.Add(nameof(paytmPGRequest.CHECKSUMHASH), paytmPGRequest.CHECKSUMHASH);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Statuscheck";
                savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, JsonConvert.SerializeObject(res), transactionPGLog.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
                paytmresp = AppWebRequest.O.PostJsonDataUsingHWR(transactionPGLog.StatuscheckURL, KeyVals);
                if (!string.IsNullOrEmpty(paytmresp))
                {
                    payresp = JsonConvert.DeserializeObject<PaytmPGResponse>(paytmresp);
                }
            }
            catch (Exception ex)
            {
                paytmresp = "Exception:" + ex.Message + "|" + paytmresp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckPG",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = transactionPGLog.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, paytmresp, transactionPGLog.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
            return payresp;
        }

        public PaytmPGResponse StatusCheckPGJS(TransactionPGLog transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var payresp = new PaytmPGResponse();
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var paytmPGRequest = new PaytmPGRequest();
            string paytmresp = string.Empty;

            try
            {
                var body = new Dictionary<string, string> {
                    {"mid",  transactionPGLog.MerchantID },
                    {"orderId", transactionPGLog.TID.ToString() }
                };

                paytmPGRequest.CHECKSUMHASH = CheckSum.generateSignature(JsonConvert.SerializeObject(body), transactionPGLog.MerchantKEY);
                var head = new Dictionary<string, string> {
                    { "signature", paytmPGRequest.CHECKSUMHASH }
                };
                var requestBody = new Dictionary<string, object> {
                    {"body", body },
                    {"head", head }
                };
                string post_data = JsonConvert.SerializeObject(requestBody);

                res.Statuscode = ErrorCodes.One;
                res.Msg = "Statuscheck";
                savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, JsonConvert.SerializeObject(res), transactionPGLog.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
                paytmresp = AppWebRequest.O.PostJsonDataUsingHWRTLS(transactionPGLog.StatuscheckURL, requestBody, head).Result;
                if (!string.IsNullOrEmpty(paytmresp))
                {
                    var payrespTemp = JsonConvert.DeserializeObject<PayTMJSPGResponse>(paytmresp);
                    if (payrespTemp != null)
                    {
                        if (payrespTemp.body != null)
                        {
                            payresp.BANKNAME = payrespTemp.body.bankName;
                            payresp.BANKTXNID = payrespTemp.body.bankTxnId;
                            payresp.CHECKSUMHASH = payrespTemp.head.signature;
                            payresp.GATEWAYNAME = payrespTemp.body.gatewayName;
                            payresp.ORDERID = payrespTemp.body.orderId;
                            payresp.STATUS = payrespTemp.body.resultInfo.resultStatus;
                            payresp.TXNAMOUNT = payrespTemp.body.txnAmount;
                            payresp.TXNID = payrespTemp.body.txnId;
                            payresp.RESPMSG = payrespTemp.body.resultInfo.resultMsg;
                            payresp.RESPCODE = payrespTemp.body.resultInfo.resultCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                paytmresp = "Exception:" + ex.Message + "|" + paytmresp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckPG",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = transactionPGLog.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, paytmresp, transactionPGLog.TransactionID, paytmPGRequest.CHECKSUMHASH, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
            return payresp;
        }
    }
}
