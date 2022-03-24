using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.CashFree
{
    public class CashFreePGML
    {
        private readonly IDAL _dal;
        private readonly string apiVersion = "2021-05-21";
        private readonly string ContentType = "application/json";
        private readonly Dictionary<string, string> paymentModes = new Dictionary<string, string>(){
            {"CCRD", "cc"},
            {"DCR", "dc"},
            {"PWLT", "PPI wallet"},
            {"NBNK", "nb"},
            {"UPI", "upi"},
            {"37UPI", "upi"},
        };
        public CashFreePGML(IDAL dal) => _dal = dal;

        public async Task<PGModelForRedirection> GeneratePGRequestForWebAsync(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                pGTransactionResponse.Domain = "https://smartindiarecharge.today";
                string paymentMode = string.Empty;
                if (paymentModes.ContainsKey(!string.IsNullOrEmpty(pGTransactionResponse.OPID) ? pGTransactionResponse.OPID : "test"))
                {
                    paymentMode = paymentModes[pGTransactionResponse.OPID];
                }
                var cashfreeOrderRequest = new CashfreeOrderRequest
                {
                    order_amount = (double)pGTransactionResponse.Amount,
                    order_currency = "INR",
                    order_id = "TID" + pGTransactionResponse.TID,
                    payment_capture = 1,
                    customer_details = new CustomerDetails
                    {
                        customer_id = pGTransactionResponse.UserID.ToString(),
                        customer_email = pGTransactionResponse.EmailID,
                        customer_phone = pGTransactionResponse.MobileNo
                    },
                    order_meta = new OrderMeta
                    {
                        payment_methods = paymentMode,
                        return_url = pGTransactionResponse.Domain + "/CashFreereturn?order_id={order_id}&order_token={order_token}",
                        notify_url = pGTransactionResponse.Domain + "/CashFreenotify"//"https://roundpay.net/Callback/45"
                    },
                    order_expiry_time = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
                CashfreeOrderResponse cashfreeResponse = new CashfreeOrderResponse();
                string baseUrl = pGTransactionResponse.URL;//"https://sandbox.cashfree.com/pg/orders";
                string clientId = pGTransactionResponse.MerchantID;
                string secretKey = pGTransactionResponse.MerchantKey;
                var headers = new Dictionary<string, string>
                {
                    {"x-client-id", clientId},
                    {"x-client-secret", secretKey},
                    {"x-api-version", apiVersion}
                };
                string reponse = await AppWebRequest.O.PostJsonDataUsingHWRTLS(baseUrl, cashfreeOrderRequest, headers).ConfigureAwait(false);
                cashfreeResponse = JsonConvert.DeserializeObject<CashfreeOrderResponse>(reponse);
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                res.URL = string.IsNullOrEmpty(cashfreeResponse.payment_link) ? cashfreeResponse.payment_link : pGTransactionResponse.Domain;
                res.CashfreeResponse = cashfreeResponse;
                //var reqRes = string.Concat(baseUrl, JsonConvert.SerializeObject(headers), JsonConvert.SerializeObject(cashfreeOrderRequest), "||", JsonConvert.SerializeObject(res));
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(cashfreeOrderRequest), pGTransactionResponse.TransactionID, JsonConvert.SerializeObject(cashfreeResponse), RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = "TID:" + pGTransactionResponse.TID + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.UserID
                });
            }
            return res;
        }

        public CashfreeStatusResponse StatusCheckPG(TransactionPGLog transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new CashfreeStatusResponse();
            //string requestedURL = transactionPGLog.StatuscheckURL;//"https://api.cashfree.com/api/v1/order/info/status";
            StringBuilder param = new StringBuilder("appId={{appId}}&secretKey={{secretKey}}&orderId={{orderId}}");
            string cashfreeresp = string.Empty;
            try
            {
                string orderId = string.Concat("TID", transactionPGLog.TID.ToString());
                param.Replace("{{appId}}", transactionPGLog.MerchantID);
                param.Replace("{{secretKey}}", transactionPGLog.MerchantKEY);
                param.Replace("{{orderId}}", orderId);
                cashfreeresp = AppWebRequest.O.CallUsingHttpWebRequest_POST(transactionPGLog.StatuscheckURL, param.ToString());
                if (!string.IsNullOrEmpty(cashfreeresp))
                {
                    res = JsonConvert.DeserializeObject<CashfreeStatusResponse>(cashfreeresp);
                    if (res != null)
                    {
                        res.orderId = orderId;
                        res.StatusCode = 1;
                        res.orderStatus = !string.IsNullOrEmpty(res.orderStatus) ? res.orderStatus : string.Empty;
                        res.status = !string.IsNullOrEmpty(res.status) ? res.status : string.Empty;
                        res.txStatus = !string.IsNullOrEmpty(res.txStatus) ? res.txStatus : string.Empty;
                        res.reason = !string.IsNullOrEmpty(res.reason) ? res.reason : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                cashfreeresp = "Exception:" + ex.Message + "|" + cashfreeresp;
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
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, string.Concat(transactionPGLog.StatuscheckURL, param), transactionPGLog.TransactionID, JsonConvert.SerializeObject(cashfreeresp), RequestMode.API, false, transactionPGLog.Amount, transactionPGLog.VendorID);
            return res ?? new CashfreeStatusResponse();
        }

        public async Task<PGModelForRedirection> GeneratePGRequestForAppAsync(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var url = new Dictionary<bool, string>
            {
                { false,"https://test.cashfree.com/api/v2/cftoken/order"},
                { true,"https://api.cashfree.com/api/v2/cftoken/order"}

            };
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            string paymentMode = string.Empty;
            if (paymentModes.ContainsKey(pGTransactionResponse.OPID))
            {
                paymentMode = paymentModes[pGTransactionResponse.OPID];
            }
            var cashfreeOrderRequest = new CashfreeOrderRequestForApp
            {
                orderAmount = Convert.ToDouble(pGTransactionResponse.Amount),
                orderCurrency = "INR",
                orderId = "TID" + pGTransactionResponse.TID
            };
            try
            {
                CashFreeResponseForApp cashfreeResponse = new CashFreeResponseForApp();
                string baseUrl = url[pGTransactionResponse.IsLive];
                string clientId = pGTransactionResponse.MerchantID;
                string secretKey = pGTransactionResponse.MerchantKey;
                var headers = new Dictionary<string, string>
                {
                    {"x-client-id", clientId},
                    {"x-client-secret", secretKey}
                };
                string reponse = await AppWebRequest.O.PostJsonDataUsingHWRTLS(baseUrl, cashfreeOrderRequest, headers).ConfigureAwait(false);
                cashfreeResponse = JsonConvert.DeserializeObject<CashFreeResponseForApp>(reponse);
                cashfreeResponse.appId = clientId;
                cashfreeResponse.orderAmount = cashfreeOrderRequest.orderAmount;
                cashfreeResponse.orderCurrency = cashfreeOrderRequest.orderCurrency;
                cashfreeResponse.orderId = cashfreeOrderRequest.orderId;
                cashfreeResponse.customerEmail = pGTransactionResponse.EmailID;
                cashfreeResponse.customerMobile = pGTransactionResponse.MobileNo;
                cashfreeResponse.notifyUrl = pGTransactionResponse.Domain + "/CashFreenotify";
                res.Statuscode = cashfreeResponse.status.Equals("ok", StringComparison.OrdinalIgnoreCase) ? ErrorCodes.One : ErrorCodes.Minus1;
                res.Msg = "Transaction intiated";
                res.CashFreeResponseForApp = cashfreeResponse;
                var reqRes = string.Concat(baseUrl, JsonConvert.SerializeObject(headers), JsonConvert.SerializeObject(cashfreeOrderRequest), "||", JsonConvert.SerializeObject(res));
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(cashfreeOrderRequest), pGTransactionResponse.TransactionID, JsonConvert.SerializeObject(cashfreeResponse), RequestMode.APPS, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForAppAsync",
                    Error = "TID:" + pGTransactionResponse.TID + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.UserID
                });
            }
            return res;
        }
    }
}